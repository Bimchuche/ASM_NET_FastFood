using ASM1_NET.Data;
using ASM1_NET.Models;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLog;
        private readonly IEmailService _emailService;

        public OrderController(AppDbContext context, IActivityLogService activityLog, IEmailService emailService)
        {
            _context = context;
            _activityLog = activityLog;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.Shipper)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Food)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            // If changing to Cancelled and order used a coupon, restore usage count
            if (Status == "Cancelled" && order.Status != "Cancelled" && order.CouponId.HasValue && order.CouponId > 0)
            {
                var coupon = await _context.Coupons.FindAsync(order.CouponId);
                if (coupon != null && coupon.UsedCount > 0)
                {
                    coupon.UsedCount--;
                }
            }

            order.Status = Status;
            if (Status == "Cancelled")
            {
                order.CancelledAt = DateTime.Now;
            }
            
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null) return NotFound();

            order.IsDeleted = true;
            order.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "Order", order.Id, order.OrderCode, $"Chuyển đơn hàng vào thùng rác: {order.OrderCode}");

            TempData["Success"] = $"Đã chuyển đơn hàng '{order.OrderCode}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 đơn hàng";
                return RedirectToAction(nameof(Index));
            }

            var orders = await _context.Orders.Where(o => ids.Contains(o.Id)).ToListAsync();
            foreach (var order in orders)
            {
                order.IsDeleted = true;
                order.DeletedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "Order", null, null, $"Xóa hàng loạt {orders.Count} đơn hàng");
            TempData["Success"] = $"Đã chuyển {orders.Count} đơn hàng vào thùng rác";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRefund(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction(nameof(Index));
            }

            if (order.PaymentStatus != "RefundPending")
            {
                TempData["Error"] = "Đơn hàng này không trong trạng thái chờ hoàn tiền";
                return RedirectToAction(nameof(Index));
            }

            order.PaymentStatus = "Refunded";
            await _context.SaveChangesAsync();

            // Send email notification to customer
            if (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Email))
            {
                var emailBody = $@"
                    <h2>Thông báo hoàn tiền</h2>
                    <p>Xin chào {order.Customer.FullName},</p>
                    <p>Đơn hàng <strong>#{order.OrderCode}</strong> của bạn đã được hoàn tiền thành công.</p>
                    <p><strong>Số tiền:</strong> {order.TotalAmount:N0}đ</p>
                    <p>Tiền sẽ được hoàn về tài khoản của bạn trong 1-3 ngày làm việc.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của FastFood!</p>
                ";
                await _emailService.SendEmailAsync(order.Customer.Email, "FastFood - Xác nhận hoàn tiền", emailBody);
            }

            await _activityLog.LogWithUserAsync("Refund", "Order", order.Id, order.OrderCode, 
                $"Xác nhận hoàn tiền cho đơn hàng #{order.OrderCode} - Số tiền: {order.TotalAmount:N0}đ");

            TempData["Success"] = $"Đã xác nhận hoàn tiền cho đơn hàng #{order.OrderCode}";
            return RedirectToAction(nameof(Index));
        }

        // View completed orders by shippers
        public async Task<IActionResult> Completed(int? shipperId)
        {
            var query = _context.Orders
                .Where(o => !o.IsDeleted && o.Status == "Completed" && o.ShipperId != null);

            if (shipperId.HasValue)
            {
                query = query.Where(o => o.ShipperId == shipperId);
            }

            var orders = await query
                .Include(o => o.Customer)
                .Include(o => o.Shipper)
                .OrderByDescending(o => o.DeliveryDate ?? o.OrderDate)
                .ToListAsync();
            
            // Get list of shippers for filter dropdown
            ViewBag.Shippers = await _context.Users
                .Where(u => u.Role == "Shipper" && !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            ViewBag.SelectedShipperId = shipperId;

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompleted(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Completed");
            }

            if (order.Status != "Completed")
            {
                TempData["Error"] = "Chỉ có thể xóa đơn hàng đã hoàn thành";
                return RedirectToAction("Completed");
            }

            order.IsDeleted = true;
            order.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "Order", order.Id, order.OrderCode, 
                $"Xóa đơn hàng đã hoàn thành: {order.OrderCode}");

            TempData["Success"] = $"Đã xóa đơn hàng #{order.OrderCode}";
            return RedirectToAction("Completed");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteCompleted(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 đơn hàng";
                return RedirectToAction("Completed");
            }

            var orders = await _context.Orders
                .Where(o => ids.Contains(o.Id) && o.Status == "Completed")
                .ToListAsync();

            foreach (var order in orders)
            {
                order.IsDeleted = true;
                order.DeletedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "Order", null, null, 
                $"Xóa hàng loạt {orders.Count} đơn hàng đã hoàn thành");

            TempData["Success"] = $"Đã xóa {orders.Count} đơn hàng";
            return RedirectToAction("Completed");
        }
    }
}
