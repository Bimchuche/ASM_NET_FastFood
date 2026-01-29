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

        // =========================
        // GET: /Order/Checkout
        // =========================
        public IActionResult Checkout()
        {
            // 1️⃣ Lấy userId từ login (INT)
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);

            // 2️⃣ Lấy cart bằng UserId (🔥 FIX LỖI)
            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Combo)
                .FirstOrDefault(c => c.UserId == userId);   // ✅ ĐÚNG

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // 3️⃣ Lấy user để fill thông tin sẵn
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                ViewBag.UserAddress = user.Address;
                ViewBag.UserPhone = user.Phone;
                ViewBag.UserName = user.FullName;
            }

            return View(cart);
        }

        // =========================
        // POST: /Order/Checkout
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string address, string phone, string paymentMethod, double? latitude, double? longitude)
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin";
                return RedirectToAction("Checkout");
            }

            // 1️⃣ Lấy userId
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);

            // 2️⃣ Lấy cart theo UserId (🔥 FIX LỖI)
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);   // ✅ ĐÚNG

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index", "Cart");
            }

            // 🆕 Auto-update user info nếu thiếu (Google OAuth users)
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

            // 3️⃣ TẠO ORDER (DÙNG CustomerId)
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
                // 🆕 Save delivery coordinates
                DeliveryLatitude = latitude,
                DeliveryLongitude = longitude
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // 4️⃣ ORDER DETAILS
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

            // 5️⃣ Xóa cart
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);

            _context.SaveChanges();

            // 6️⃣ Ghi log hoạt động tạo đơn hàng
            await _activityLog.LogAsync(
                "Order", 
                "Order", 
                order.Id, 
                order.OrderCode, 
                $"Khách hàng {user?.FullName ?? "Unknown"} đặt đơn hàng #{order.OrderCode} - Tổng: {order.TotalAmount:N0}đ"
            );

            return RedirectToAction("Success", new { id = order.Id });
        }

        // =========================
        // GET: /Order/Success
        // =========================
        public IActionResult Success(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Combo)
                .FirstOrDefault(o => o.Id == id && !o.IsDeleted);  // ✅ Ẩn đơn đã xóa

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
        // =========================
        // GET: /Order/History
        // =========================
        public IActionResult History()
        {
            // 1️⃣ Lấy userId từ login (INT)
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);

            // 2️⃣ Lấy danh sách order của khách
            var orders = _context.Orders
                .Where(o => o.CustomerId == userId && !o.IsDeleted)  // ✅ Ẩn đơn đã xóa
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // =========================
        // POST: /Order/Cancel
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            // 1️⃣ Lấy userId từ login
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(claim.Value);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            // 2️⃣ Tìm đơn hàng
            var order = _context.Orders.FirstOrDefault(o => o.Id == id && o.CustomerId == userId && !o.IsDeleted);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("History");
            }

            // 3️⃣ Chỉ cho hủy khi đơn còn Pending
            if (order.Status != "Pending")
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng đang chờ xử lý";
                return RedirectToAction("History");
            }

            // 4️⃣ Cập nhật trạng thái
            order.Status = "Cancelled";
            _context.SaveChanges();

            // 5️⃣ Ghi log hoạt động hủy đơn hàng
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
