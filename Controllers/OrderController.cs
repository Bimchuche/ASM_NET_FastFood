using ASM1_NET.Data;
using ASM1_NET.Models;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASM1_NET.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLog;
        private readonly IConfiguration _configuration;
        private readonly ILoyaltyService _loyaltyService;

        public OrderController(AppDbContext context, IActivityLogService activityLog, IConfiguration configuration, ILoyaltyService loyaltyService)
        {
            _context = context;
            _activityLog = activityLog;
            _configuration = configuration;
            _loyaltyService = loyaltyService;
        }

        public IActionResult Checkout()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Combo)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                ViewBag.UserPhone = user.Phone;
                ViewBag.UserName = user.FullName;
            }

            // Load saved addresses
            var addresses = _context.UserAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();
            ViewBag.SavedAddresses = addresses;
            
            // Get default address
            var defaultAddr = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
            if (defaultAddr != null)
            {
                ViewBag.DefaultAddress = defaultAddr;
                ViewBag.UserAddress = defaultAddr.FullAddress;
            }
            else if (user != null)
            {
                ViewBag.UserAddress = user.Address;
            }

            return View(cart);
        }

        // API tính phí shipping dựa trên khoảng cách
        [HttpGet]
        public async Task<IActionResult> GetShippingFee(double distance)
        {
            // Lấy zone phù hợp với khoảng cách
            var zone = await _context.ShippingZones
                .Where(z => z.IsActive && distance >= z.MinDistance && distance < z.MaxDistance)
                .FirstOrDefaultAsync();

            if (zone != null)
            {
                return Json(new { 
                    success = true, 
                    fee = zone.ShippingFee,
                    zoneName = zone.Name
                });
            }

            // Nếu không có zone, tính theo công thức: 15,000đ + 5,000đ/km sau 3km đầu
            decimal baseFee = 15000;
            if (distance > 3)
            {
                baseFee += (decimal)(distance - 3) * 5000;
            }
            
            return Json(new { 
                success = true, 
                fee = baseFee,
                zoneName = "Tính theo khoảng cách"
            });
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCoupon(string code, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá!" });
            }

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code.ToUpper() && !c.IsDeleted);

            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại!" });
            }

            if (!coupon.IsActive)
            {
                return Json(new { success = false, message = "Mã giảm giá đã bị vô hiệu hóa!" });
            }

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn!" });
            }

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng!" });
            }

            if (coupon.MinOrderAmount.HasValue && totalAmount < coupon.MinOrderAmount)
            {
                return Json(new { 
                    success = false, 
                    message = $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0}₫ để áp dụng mã này!" 
                });
            }

            // Calculate discount
            var discountAmount = totalAmount * coupon.DiscountPercent / 100;
            if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount)
            {
                discountAmount = coupon.MaxDiscountAmount.Value;
            }

            return Json(new { 
                success = true, 
                message = $"Giảm {coupon.DiscountPercent}% (-{discountAmount:N0}₫)",
                discountAmount = discountAmount,
                couponId = coupon.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string address, string phone, string paymentMethod, double? latitude, double? longitude, int? couponId, decimal discountAmount = 0, decimal shippingFee = 0)
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction("Checkout");
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                bool needUpdate = false;
                
                if (string.IsNullOrWhiteSpace(user.Phone) && !string.IsNullOrWhiteSpace(phone))
                {
                    user.Phone = phone;
                    needUpdate = true;
                }
                
                if (string.IsNullOrWhiteSpace(user.Address) && !string.IsNullOrWhiteSpace(address))
                {
                    user.Address = address;
                    needUpdate = true;
                }
                
                if (needUpdate)
                {
                    _context.SaveChanges();
                }
            }

            // Calculate total with shipping and discount
            var subtotal = cart.CartItems.Sum(i => i.Price * i.Quantity);
            var finalTotal = subtotal + shippingFee - discountAmount;
            if (finalTotal < 0) finalTotal = 0;

            // Update coupon usage if applied
            if (couponId.HasValue && couponId > 0)
            {
                var coupon = await _context.Coupons.FindAsync(couponId);
                if (coupon != null)
                {
                    coupon.UsedCount++;
                    HttpContext.Session.SetString("PendingOrder_CouponId", couponId.ToString());
                    HttpContext.Session.SetString("PendingOrder_Discount", discountAmount.ToString());
                }
            }

            // For QR payment - redirect to PayOS first, create order after payment
            if (paymentMethod == "QR")
            {
                // Store order info in session for later
                HttpContext.Session.SetString("PendingOrder_Address", address);
                HttpContext.Session.SetString("PendingOrder_Phone", phone);
                HttpContext.Session.SetString("PendingOrder_UserId", userId.ToString());
                HttpContext.Session.SetString("PendingOrder_FinalTotal", finalTotal.ToString());
                if (latitude.HasValue) HttpContext.Session.SetString("PendingOrder_Lat", latitude.Value.ToString());
                if (longitude.HasValue) HttpContext.Session.SetString("PendingOrder_Lng", longitude.Value.ToString());
                
                return RedirectToAction("CreatePayOSLink", "Payment");
            }

            // COD - create order immediately
            var order = new Order
            {
                OrderCode = "ORD" + DateTime.Now.Ticks,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Address = address,
                Phone = phone,
                PaymentMethod = "COD",
                CustomerId = userId,
                TotalAmount = finalTotal,
                DeliveryLatitude = latitude,
                DeliveryLongitude = longitude,
                CouponId = couponId > 0 ? couponId : null,
                DiscountAmount = discountAmount
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

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

            _context.SaveChanges();

            await _activityLog.LogAsync(
                "Order", 
                "Order", 
                order.Id, 
                order.OrderCode, 
                $"Khách hàng {user?.FullName ?? "Unknown"} đặt đơn hàng #{order.OrderCode} - Tổng: {order.TotalAmount:N0}đ"
            );

            return RedirectToAction("Success", new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Combo)
                .FirstOrDefault(o => o.Id == id && !o.IsDeleted);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public IActionResult Detail(int id)
        {
            var food = _context.Foods.FirstOrDefault(f => f.Id == id);

            if (food == null)
                return NotFound();

            return View(food);
        }

        public IActionResult History()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                ViewBag.RequireLogin = true;
                return View(new List<Order>());
            }

            int userId = int.Parse(claim.Value);

            var orders = _context.Orders
                .Where(o => o.CustomerId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            var order = _context.Orders.FirstOrDefault(o => o.Id == id && o.CustomerId == userId && !o.IsDeleted);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("History");
            }

            // Chỉ cho phép hủy khi shipper chưa nhận đơn
            var allowedStatuses = new[] { "Pending", "Đã thanh toán", "Đang xử lý" };
            if (!allowedStatuses.Contains(order.Status))
            {
                TempData["Error"] = "Không thể hủy đơn hàng. Shipper đã nhận đơn hoặc đơn hàng đã hoàn thành.";
                return RedirectToAction("History");
            }

            // Nếu là thanh toán QR đã thanh toán -> đánh dấu cần hoàn tiền (thủ công qua PayOS dashboard)
            string refundMessage = "";
            
            if (order.PaymentMethod == "QR" && order.PaymentStatus == "Paid")
            {
                // PayOS Cancel API chỉ hủy link chưa thanh toán
                // Giao dịch đã PAID cần hoàn tiền thủ công qua PayOS Dashboard
                order.PaymentStatus = "RefundPending";
                refundMessage = "Đơn hàng đang chờ hoàn tiền. Tiền sẽ được hoàn trong thời gian sớm nhất có thể.";
            }

            // Restore coupon usage if order used a coupon
            if (order.CouponId.HasValue && order.CouponId > 0)
            {
                var coupon = await _context.Coupons.FindAsync(order.CouponId);
                if (coupon != null && coupon.UsedCount > 0)
                {
                    coupon.UsedCount--;
                }
            }

            order.Status = "Cancelled";
            order.CancelledAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                "Cancel", 
                "Order", 
                order.Id, 
                order.OrderCode, 
                $"Khách hàng {user?.FullName ?? "Unknown"} hủy đơn hàng #{order.OrderCode}" + 
                (order.PaymentStatus == "RefundPending" ? " - Chờ hoàn tiền" : "")
            );

            if (order.PaymentMethod == "QR" && order.PaymentStatus == "RefundPending")
            {
                TempData["Success"] = $"Đã hủy đơn hàng thành công. {refundMessage}";
            }
            else
            {
                TempData["Success"] = "Đã hủy đơn hàng thành công";
            }
            
            return RedirectToAction("History");
        }
    }
}
