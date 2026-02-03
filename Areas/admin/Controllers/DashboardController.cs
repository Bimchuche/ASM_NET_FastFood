using ASM1_NET.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            // Basic stats
            ViewBag.TotalCategory = _context.Categories.Count(c => !c.IsDeleted);
            ViewBag.TotalFood = _context.Foods.Count(f => !f.IsDeleted);
            ViewBag.TotalCombo = _context.Combos.Count(c => !c.IsDeleted);
            ViewBag.TotalOrder = _context.Orders.Count(o => !o.IsDeleted);
            ViewBag.TotalUser = _context.Users.Count(u => !u.IsDeleted);

            // Revenue stats
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            ViewBag.TodayRevenue = _context.Orders
                .Where(o => o.OrderDate.Date == today && o.Status == "Completed" && !o.IsDeleted)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.MonthRevenue = _context.Orders
                .Where(o => o.OrderDate >= thisMonth && o.Status == "Completed" && !o.IsDeleted)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.TotalRevenue = _context.Orders
                .Where(o => o.Status == "Completed" && !o.IsDeleted)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Order status counts
            ViewBag.PendingOrders = _context.Orders.Count(o => o.Status == "Pending" && !o.IsDeleted);
            ViewBag.DeliveringOrders = _context.Orders.Count(o => o.Status == "Delivering" && !o.IsDeleted);
            ViewBag.CompletedOrders = _context.Orders.Count(o => o.Status == "Completed" && !o.IsDeleted);
            ViewBag.CancelledOrders = _context.Orders.Count(o => o.Status == "Cancelled" && !o.IsDeleted);

            // Category chart
            ViewBag.CategoryNames = _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => c.Name)
                .ToList();

            ViewBag.FoodCounts = _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => _context.Foods.Count(f => f.CategoryId == c.Id && !f.IsDeleted))
                .ToList();

            // Latest orders
            ViewBag.LatestOrders = _context.Orders
                .Include(o => o.Customer)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            // Top selling foods
            var topFoods = _context.OrderDetails
                .Include(od => od.Food)
                .Where(od => od.Food != null && od.Order.Status == "Completed")
                .GroupBy(od => od.FoodId)
                .Select(g => new { FoodId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalQty)
                .Take(5)
                .ToList();

            ViewBag.TopFoods = topFoods.Select(t => new {
                Name = _context.Foods.FirstOrDefault(f => f.Id == t.FoodId)?.Name ?? "N/A",
                Quantity = t.TotalQty
            }).ToList();

            return View();
        }

        // API: Revenue by date range
        [HttpGet]
        public IActionResult RevenueByDate(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? DateTime.Today.AddDays(-7);
            var toDate = to ?? DateTime.Today;

            var data = _context.Orders
        .Where(o => o.OrderDate.Date >= fromDate.Date && o.OrderDate.Date <= toDate.Date 
                    && o.Status == "Completed" && !o.IsDeleted)
        .GroupBy(o => o.OrderDate.Date)
        .Select(g => new { 
            Date = g.Key, 
            Revenue = g.Sum(x => x.TotalAmount) 
        })
        .OrderBy(x => x.Date)
        .ToList()
        .Select(x => new { Date = x.Date.ToString("dd/MM"), x.Revenue })
        .ToList();

            return Json(data);
        }

        // API: Order stats by status
        [HttpGet]
        public IActionResult OrderStats()
        {
            var stats = new
            {
                Pending = _context.Orders.Count(o => o.Status == "Pending" && !o.IsDeleted),
                Delivering = _context.Orders.Count(o => o.Status == "Delivering" && !o.IsDeleted),
                Completed = _context.Orders.Count(o => o.Status == "Completed" && !o.IsDeleted),
                Cancelled = _context.Orders.Count(o => o.Status == "Cancelled" && !o.IsDeleted)
            };
            return Json(stats);
        }

        // API: Top selling products
        [HttpGet]
        public IActionResult TopProducts(int count = 5)
        {
            var topFoods = _context.OrderDetails
                .Include(od => od.Food)
                .Where(od => od.Food != null && od.Order.Status == "Completed")
                .GroupBy(od => od.FoodId)
                .Select(g => new { FoodId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalQty)
                .Take(count)
                .ToList();

            var result = topFoods.Select(t => new {
                Name = _context.Foods.FirstOrDefault(f => f.Id == t.FoodId)?.Name ?? "N/A",
                Quantity = t.TotalQty
            }).ToList();

            return Json(result);
        }
    }
}
