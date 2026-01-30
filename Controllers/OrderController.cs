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

        public OrderController(AppDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
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
                ViewBag.UserAddress = user.Address;
                ViewBag.UserPhone = user.Phone;
                ViewBag.UserName = user.FullName;
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string address, string phone, string paymentMethod, double? latitude, double? longitude)
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

            var order = new Order
            {
                OrderCode = "ORD" + DateTime.Now.Ticks,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Address = address,
                Phone = phone,
                PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "COD" : paymentMethod,
                CustomerId = userId,
                TotalAmount = cart.CartItems.Sum(i => i.Price * i.Quantity),
                DeliveryLatitude = latitude,
                DeliveryLongitude = longitude
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
                return RedirectToAction("Login", "Account");

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

            if (order.Status != "Pending")
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng đang chờ xử lý";
                return RedirectToAction("History");
            }

            order.Status = "Cancelled";
            _context.SaveChanges();

            await _activityLog.LogAsync(
                "Cancel", 
                "Order", 
                order.Id, 
                order.OrderCode, 
                $"Khách hàng {user?.FullName ?? "Unknown"} hủy đơn hàng #{order.OrderCode}"
            );

            TempData["Success"] = "Đã hủy đơn hàng thành công";
            return RedirectToAction("History");
        }
    }
}
