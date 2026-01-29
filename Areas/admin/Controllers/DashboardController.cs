using ASM1_NET.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ASM1_NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Thống kê tổng
            ViewBag.TotalCategory = _context.Categories.Count();
            ViewBag.TotalFood = _context.Foods.Count();
            ViewBag.TotalCombo = _context.Combos.Count();
            ViewBag.TotalOrder = _context.Orders.Count();

            // ===== DATA CHO CHART =====
            ViewBag.CategoryNames = _context.Categories
                .Select(c => c.Name)
                .ToList();

            ViewBag.FoodCounts = _context.Categories
                .Select(c => _context.Foods.Count(f => f.CategoryId == c.Id))
                .ToList();

            // ===== ĐƠN HÀNG MỚI NHẤT =====
            ViewBag.LatestOrders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
