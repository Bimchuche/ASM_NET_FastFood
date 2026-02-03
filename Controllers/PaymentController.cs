using ASM1_NET.Data;
using ASM1_NET.Models;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ASM1_NET.Controllers;

public class PaymentController : Controller
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IActivityLogService _activityLog;
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly string _checksumKey;

    public PaymentController(AppDbContext context, IConfiguration configuration, IActivityLogService activityLog)
    {
        _context = context;
        _configuration = configuration;
        _activityLog = activityLog;
        _httpClient = new HttpClient();
        
        _clientId = configuration["PayOS:ClientId"] ?? "";
        _apiKey = configuration["PayOS:ApiKey"] ?? "";
        _checksumKey = configuration["PayOS:ChecksumKey"] ?? "";
        
        _httpClient.BaseAddress = new Uri("https://api-merchant.payos.vn");
        _httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }

    [HttpGet]
    public async Task<IActionResult> CreatePayOSLink()
    {
        try
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Food)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Combo)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // Get final total from session (includes shipping fee and discount)
            var finalTotalStr = HttpContext.Session.GetString("PendingOrder_FinalTotal");
            int totalAmount;
            if (!string.IsNullOrEmpty(finalTotalStr) && decimal.TryParse(finalTotalStr, out var parsed))
            {
                totalAmount = (int)parsed;
            }
            else
            {
                // Fallback to cart total if session doesn't have it
                totalAmount = (int)cart.CartItems.Sum(i => i.Price * i.Quantity);
            }
            
            var orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
            var domain = $"{Request.Scheme}://{Request.Host}";
            var description = "FastFood";
            var returnUrl = $"{domain}/Payment/Success?orderCode={orderCode}";
            var cancelUrl = $"{domain}/Payment/Cancel";

            var signatureData = $"amount={totalAmount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            var signature = ComputeHmacSha256(_checksumKey, signatureData);

            var items = cart.CartItems.Select(ci => new
            {
                name = ci.Food?.Name ?? ci.Combo?.Name ?? "Sản phẩm",
                quantity = ci.Quantity,
                price = (int)ci.Price
            }).ToList();

            var requestBody = new
            {
                orderCode = orderCode,
                amount = totalAmount,
                description = description,
                items = items,
                returnUrl = returnUrl,
                cancelUrl = cancelUrl,
                signature = signature
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v2/payment-requests", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            if (result.TryGetProperty("code", out var codeEl) && codeEl.GetString() == "00")
            {
                var data = result.GetProperty("data");
                var checkoutUrl = data.GetProperty("checkoutUrl").GetString();
                
                HttpContext.Session.SetString("PayOS_OrderCode", orderCode.ToString());
                
                return Redirect(checkoutUrl!);
            }
            else
            {
                var desc = result.TryGetProperty("desc", out var descEl) ? descEl.GetString() : "Lỗi tạo link thanh toán";
                TempData["Error"] = desc;
                return RedirectToAction("Checkout", "Order");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi: {ex.Message}";
            return RedirectToAction("Checkout", "Order");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(long orderCode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v2/payment-requests/{orderCode}");
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

            if (result.TryGetProperty("code", out var codeEl) && codeEl.GetString() == "00")
            {
                var data = result.GetProperty("data");
                var status = data.GetProperty("status").GetString();

                if (status == "PAID")
                {
                    var address = HttpContext.Session.GetString("PendingOrder_Address") ?? "";
                    var phone = HttpContext.Session.GetString("PendingOrder_Phone") ?? "";
                    var userIdStr = HttpContext.Session.GetString("PendingOrder_UserId");
                    var latStr = HttpContext.Session.GetString("PendingOrder_Lat");
                    var lngStr = HttpContext.Session.GetString("PendingOrder_Lng");

                    if (string.IsNullOrEmpty(userIdStr))
                    {
                        TempData["Error"] = "Phiên đã hết hạn. Vui lòng đặt hàng lại.";
                        return RedirectToAction("Checkout", "Order");
                    }

                    int userId = int.Parse(userIdStr);
                    double? lat = string.IsNullOrEmpty(latStr) ? null : double.Parse(latStr);
                    double? lng = string.IsNullOrEmpty(lngStr) ? null : double.Parse(lngStr);

                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart == null || !cart.CartItems.Any())
                    {
                        TempData["Error"] = "Giỏ hàng trống";
                        return RedirectToAction("Index", "Cart");
                    }

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    var order = new Order
                    {
                        OrderCode = "ORD" + DateTime.Now.Ticks,
                        OrderDate = DateTime.Now,
                        Status = "Đã thanh toán",
                        Address = address,
                        Phone = phone,
                        PaymentMethod = "QR",
                        PaymentStatus = "Paid",
                        PaymentOrderCode = orderCode,
                        ConfirmedAt = DateTime.Now,
                        CustomerId = userId,
                        TotalAmount = cart.CartItems.Sum(i => i.Price * i.Quantity),
                        DeliveryLatitude = lat,
                        DeliveryLongitude = lng
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in cart.CartItems)
                    {
                        _context.OrderDetails.Add(new OrderDetail
                        {
                            OrderId = order.Id,
                            FoodId = item.FoodId,
                            ComboId = item.ComboId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price
                        });
                    }

                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.Carts.Remove(cart);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove("PendingOrder_Address");
                    HttpContext.Session.Remove("PendingOrder_Phone");
                    HttpContext.Session.Remove("PendingOrder_UserId");
                    HttpContext.Session.Remove("PendingOrder_Lat");
                    HttpContext.Session.Remove("PendingOrder_Lng");
                    HttpContext.Session.Remove("PayOS_OrderCode");

                    await _activityLog.LogAsync(
                        "Order", 
                        "Order", 
                        order.Id, 
                        order.OrderCode, 
                        $"Khách hàng {user?.FullName ?? "Unknown"} đặt đơn hàng #{order.OrderCode} - Thanh toán QR - Tổng: {order.TotalAmount:N0}đ"
                    );

                    TempData["Success"] = "Thanh toán thành công! Đơn hàng của bạn đã được tạo.";
                    return RedirectToAction("Success", "Order", new { id = order.Id });
                }
            }

            TempData["Error"] = "Thanh toán chưa hoàn tất. Vui lòng thử lại.";
            return RedirectToAction("Checkout", "Order");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi xác nhận thanh toán: {ex.Message}";
            return RedirectToAction("Checkout", "Order");
        }
    }

    [HttpGet]
    public IActionResult Cancel()
    {
        TempData["Error"] = "Thanh toán đã bị hủy. Bạn có thể chọn phương thức thanh toán khác.";
        return RedirectToAction("Checkout", "Order");
    }

    [HttpPost]
    [Route("api/payment/webhook")]
    public async Task<IActionResult> Webhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            
            var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            
            if (root.TryGetProperty("code", out var codeElement) && codeElement.GetString() == "00")
            {
                if (root.TryGetProperty("data", out var dataElement) &&
                    dataElement.TryGetProperty("orderCode", out var orderCodeElement))
                {
                    var orderCode = orderCodeElement.GetInt64();
                    
                    var order = await _context.Orders
                        .FirstOrDefaultAsync(o => o.PaymentOrderCode == orderCode);

                    if (order != null)
                    {
                        order.Status = "Đã thanh toán";
                        order.PaymentStatus = "Paid";
                        order.ConfirmedAt = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckStatus(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
        }

        if (order.PaymentOrderCode == null)
        {
            return Json(new { success = false, paid = false, status = "NoPayment" });
        }

        try
        {
            var response = await _httpClient.GetAsync($"/v2/payment-requests/{order.PaymentOrderCode}");
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

            if (result.TryGetProperty("code", out var codeEl) && codeEl.GetString() == "00")
            {
                var data = result.GetProperty("data");
                var status = data.GetProperty("status").GetString();

                if (status == "PAID")
                {
                    order.Status = "Đã thanh toán";
                    order.PaymentStatus = "Paid";
                    order.ConfirmedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    return Json(new { success = true, paid = true, status = status });
                }

                return Json(new { success = true, paid = false, status = status });
            }

            return Json(new { success = false, message = "Không thể kiểm tra trạng thái" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private static string ComputeHmacSha256(string key, string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
