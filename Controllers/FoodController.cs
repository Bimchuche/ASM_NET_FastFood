using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM1_NET.Data;
using ASM1_NET.Helpers;

namespace ASM1_NET.Controllers
{
    public class FoodController : Controller
    {
        private readonly AppDbContext _context;

        public FoodController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(
            string? keyword, 
            int? categoryId, 
            decimal? minPrice, 
            decimal? maxPrice,
            int? minRating,
            string? sortBy,
            int page = 1,
            int pageSize = 12)
        {
            var query = _context.Foods
                .Include(f => f.Category)
                .Include(f => f.Reviews)
                .Where(f => f.IsAvailable && !f.IsDeleted);

            // Filter by category
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(f =>
                    f.Name.ToLower().Contains(keyword) ||
                    (f.Description ?? "").ToLower().Contains(keyword));
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(f => f.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(f => f.Price <= maxPrice.Value);
            }

            // Filter by rating
            if (minRating.HasValue && minRating > 0)
            {
                query = query.Where(f => f.Reviews.Any() && 
                    f.Reviews.Average(r => r.Rating) >= minRating.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(f => f.Price),
                "price_desc" => query.OrderByDescending(f => f.Price),
                "name_asc" => query.OrderBy(f => f.Name),
                "name_desc" => query.OrderByDescending(f => f.Name),
                "rating" => query.OrderByDescending(f => f.Reviews.Any() ? f.Reviews.Average(r => r.Rating) : 0),
                "newest" => query.OrderByDescending(f => f.Id),
                _ => query.OrderBy(f => f.Name)
            };

            // Pagination
            var totalCount = query.Count();
            var foods = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ViewBag for filters
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinRating = minRating;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive && !c.IsDeleted).ToList();

            // Get wishlist items for current user
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                ViewBag.WishlistIds = _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.FoodId)
                    .ToList();
            }
            else
            {
                ViewBag.WishlistIds = new List<int>();
            }

            return View(foods);
        }

        public IActionResult DetailPopup(int id)
        {
            var food = _context.Foods
                .Include(f => f.Category)
                .Include(f => f.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(f => f.Id == id && f.IsAvailable && !f.IsDeleted);
            if (food == null) return NotFound();

            return PartialView("_FoodDetailPopup", food);
        }
    }
}
