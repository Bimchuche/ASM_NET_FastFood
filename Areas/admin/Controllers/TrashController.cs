using ASM1_NET.Data;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý Thùng rác (Soft Delete)
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrashController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public TrashController(AppDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        /// <summary>
        /// Trang Thùng rác chính - hiển thị tất cả items đã xóa
        /// </summary>
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Thùng rác";

            var deletedFoods = await _context.Foods.Where(f => f.IsDeleted).CountAsync();
            var deletedCombos = await _context.Combos.Where(c => c.IsDeleted).CountAsync();
            var deletedCategories = await _context.Categories.Where(c => c.IsDeleted).CountAsync();
            var deletedUsers = await _context.Users.Where(u => u.IsDeleted).CountAsync();
            var deletedOrders = await _context.Orders.Where(o => o.IsDeleted).CountAsync();

            ViewBag.DeletedFoods = deletedFoods;
            ViewBag.DeletedCombos = deletedCombos;
            ViewBag.DeletedCategories = deletedCategories;
            ViewBag.DeletedUsers = deletedUsers;
            ViewBag.DeletedOrders = deletedOrders;
            ViewBag.Total = deletedFoods + deletedCombos + deletedCategories + deletedUsers + deletedOrders;

            return View();
        }

        // ================= FOOD =================
        public async Task<IActionResult> Foods()
        {
            ViewData["Title"] = "Món ăn đã xóa";
            var foods = await _context.Foods
                .Where(f => f.IsDeleted)
                .Include(f => f.Category)
                .OrderByDescending(f => f.DeletedAt)
                .ToListAsync();
            return View(foods);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                food.IsDeleted = false;
                food.DeletedAt = null;
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Restore", "Food", id, food.Name, $"Khôi phục món ăn: {food.Name}");
                TempData["Success"] = $"Đã khôi phục món ăn '{food.Name}'";
            }
            return RedirectToAction(nameof(Foods));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                var name = food.Name;
                _context.Foods.Remove(food);
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Delete", "Food", id, name, $"Xóa vĩnh viễn món ăn: {name}");
                TempData["Success"] = $"Đã xóa vĩnh viễn món ăn '{name}'";
            }
            return RedirectToAction(nameof(Foods));
        }

        // ================= COMBO =================
        public async Task<IActionResult> Combos()
        {
            ViewData["Title"] = "Combo đã xóa";
            var combos = await _context.Combos
                .Where(c => c.IsDeleted)
                .OrderByDescending(c => c.DeletedAt)
                .ToListAsync();
            return View(combos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCombo(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                combo.IsDeleted = false;
                combo.DeletedAt = null;
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Restore", "Combo", id, combo.Name, $"Khôi phục combo: {combo.Name}");
                TempData["Success"] = $"Đã khôi phục combo '{combo.Name}'";
            }
            return RedirectToAction(nameof(Combos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteCombo(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                var name = combo.Name;
                _context.Combos.Remove(combo);
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Delete", "Combo", id, name, $"Xóa vĩnh viễn combo: {name}");
                TempData["Success"] = $"Đã xóa vĩnh viễn combo '{name}'";
            }
            return RedirectToAction(nameof(Combos));
        }

        // ================= CATEGORY =================
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Danh mục đã xóa";
            var categories = await _context.Categories
                .Where(c => c.IsDeleted)
                .OrderByDescending(c => c.DeletedAt)
                .ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.IsDeleted = false;
                category.DeletedAt = null;
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Restore", "Category", id, category.Name, $"Khôi phục danh mục: {category.Name}");
                TempData["Success"] = $"Đã khôi phục danh mục '{category.Name}'";
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                var name = category.Name;
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Delete", "Category", id, name, $"Xóa vĩnh viễn danh mục: {name}");
                TempData["Success"] = $"Đã xóa vĩnh viễn danh mục '{name}'";
            }
            return RedirectToAction(nameof(Categories));
        }

        // ================= USER =================
        public async Task<IActionResult> Users()
        {
            ViewData["Title"] = "Người dùng đã xóa";
            var users = await _context.Users
                .Where(u => u.IsDeleted)
                .OrderByDescending(u => u.DeletedAt)
                .ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = false;
                user.DeletedAt = null;
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Restore", "User", id, user.FullName, $"Khôi phục user: {user.FullName}");
                TempData["Success"] = $"Đã khôi phục user '{user.FullName}'";
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                var name = user.FullName;
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Delete", "User", id, name, $"Xóa vĩnh viễn user: {name}");
                TempData["Success"] = $"Đã xóa vĩnh viễn user '{name}'";
            }
            return RedirectToAction(nameof(Users));
        }

        // ================= ORDER =================
        public async Task<IActionResult> Orders()
        {
            ViewData["Title"] = "Đơn hàng đã xóa";
            var orders = await _context.Orders
                .Where(o => o.IsDeleted)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.DeletedAt)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.IsDeleted = false;
                order.DeletedAt = null;
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Restore", "Order", id, order.OrderCode, $"Khôi phục đơn hàng: {order.OrderCode}");
                TempData["Success"] = $"Đã khôi phục đơn hàng '{order.OrderCode}'";
            }
            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                var code = order.OrderCode;
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await _activityLogService.LogWithUserAsync("Delete", "Order", id, code, $"Xóa vĩnh viễn đơn hàng: {code}");
                TempData["Success"] = $"Đã xóa vĩnh viễn đơn hàng '{code}'";
            }
            return RedirectToAction(nameof(Orders));
        }

        // ================= EMPTY TRASH =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmptyAll()
        {
            // Xóa tất cả items trong trash
            var foods = await _context.Foods.Where(f => f.IsDeleted).ToListAsync();
            var combos = await _context.Combos.Where(c => c.IsDeleted).ToListAsync();
            var categories = await _context.Categories.Where(c => c.IsDeleted).ToListAsync();
            var users = await _context.Users.Where(u => u.IsDeleted).ToListAsync();
            var orders = await _context.Orders.Where(o => o.IsDeleted).ToListAsync();

            _context.Foods.RemoveRange(foods);
            _context.Combos.RemoveRange(combos);
            _context.Categories.RemoveRange(categories);
            _context.Users.RemoveRange(users);
            _context.Orders.RemoveRange(orders);

            await _context.SaveChangesAsync();

            var total = foods.Count + combos.Count + categories.Count + users.Count + orders.Count;
            await _activityLogService.LogWithUserAsync("Delete", null, null, null, $"Đã dọn sạch thùng rác: {total} items");
            
            TempData["Success"] = $"Đã dọn sạch thùng rác ({total} items)";
            return RedirectToAction(nameof(Index));
        }
    }
}
