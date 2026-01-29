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

        public OrderController(AppDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // 📌 DANH SÁCH ĐƠN
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Where(o => !o.IsDeleted)  // ✅ Filter soft deleted
                .Include(o => o.Customer)
                .Include(o => o.Shipper)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 📌 CHI TIẾT ĐƠN
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

        // 📌 SỬA ĐƠN (chỉ sửa trạng thái)
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

            order.Status = Status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 📌 XOÁ ĐƠN
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

            // ✅ SOFT DELETE
            order.IsDeleted = true;
            order.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "Order", order.Id, order.OrderCode, $"Chuyển đơn hàng vào thùng rác: {order.OrderCode}");

            TempData["Success"] = $"Đã chuyển đơn hàng '{order.OrderCode}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }
    }
}
