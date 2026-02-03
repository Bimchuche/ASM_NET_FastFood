using System.Security.Claims;
using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // Trang hiển thị tất cả đánh giá
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Food)
                .Include(r => r.Combo)
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Combo)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("History", "Order");
            }

            if (order.Status != "Completed")
            {
                TempData["Error"] = "Chỉ có thể đánh giá đơn hàng đã hoàn thành!";
                return RedirectToAction("History", "Order");
            }

            // Check if already reviewed
            var existingReview = await _context.Reviews.AnyAsync(r => r.OrderId == orderId && r.UserId == userId);
            if (existingReview)
            {
                TempData["Error"] = "Bạn đã đánh giá đơn hàng này rồi!";
                return RedirectToAction("History", "Order");
            }

            ViewBag.Order = order;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderId, int rating, string? comment)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId && o.Status == "Completed");

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc đơn hàng chưa hoàn thành!";
                return RedirectToAction("History", "Order");
            }

            // Check if already reviewed
            var existingReview = await _context.Reviews.AnyAsync(r => r.OrderId == orderId && r.UserId == userId);
            if (existingReview)
            {
                TempData["Error"] = "Bạn đã đánh giá đơn hàng này rồi!";
                return RedirectToAction("History", "Order");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating phải từ 1 đến 5 sao!";
                return RedirectToAction("Create", new { orderId });
            }

            // Create reviews for each item in order
            foreach (var detail in order.OrderDetails)
            {
                var review = new Review
                {
                    OrderId = orderId,
                    UserId = userId,
                    FoodId = detail.FoodId,
                    ComboId = detail.ComboId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };
                _context.Reviews.Add(review);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá! ⭐";
            return RedirectToAction("History", "Order");
        }

        // API để lấy rating trung bình của món ăn
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetFoodRating(int foodId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.FoodId == foodId)
                .ToListAsync();

            if (!reviews.Any())
            {
                return Json(new { avgRating = 0, count = 0 });
            }

            var avgRating = reviews.Average(r => r.Rating);
            return Json(new { avgRating = Math.Round(avgRating, 1), count = reviews.Count });
        }

        // API để lấy rating trung bình của combo
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetComboRating(int comboId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ComboId == comboId)
                .ToListAsync();

            if (!reviews.Any())
            {
                return Json(new { avgRating = 0, count = 0 });
            }

            var avgRating = reviews.Average(r => r.Rating);
            return Json(new { avgRating = Math.Round(avgRating, 1), count = reviews.Count });
        }
    }
}
