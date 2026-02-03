using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM1_NET.Data;

namespace ASM1_NET.Controllers
{
    public class ComboController : Controller
    {
        private readonly AppDbContext _context;

        public ComboController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string keyword, string sortBy, decimal? minPrice, decimal? maxPrice, int? minRating, int page = 1)
        {
            int pageSize = 8;
            
            var query = _context.Combos
                .Where(c => c.IsActive && !c.IsDeleted)
                .AsQueryable();

            // Keyword search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.Name.Contains(keyword) || c.Description.Contains(keyword));
            }

            // Price filter
            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            // Get combo IDs first for rating calculation
            var comboIds = query.Select(c => c.Id).ToList();
            
            // Calculate ratings for each combo
            var comboRatings = _context.Reviews
                .Where(r => r.ComboId != null && comboIds.Contains(r.ComboId.Value))
                .GroupBy(r => r.ComboId)
                .Select(g => new {
                    ComboId = g.Key,
                    AvgRating = g.Average(r => r.Rating),
                    ReviewCount = g.Count()
                })
                .ToDictionary(x => x.ComboId ?? 0, x => new { x.AvgRating, x.ReviewCount });

            // Filter by rating
            if (minRating.HasValue && minRating.Value > 0)
            {
                var combosWithMinRating = comboRatings
                    .Where(x => x.Value.AvgRating >= minRating.Value)
                    .Select(x => x.Key)
                    .ToList();
                query = query.Where(c => combosWithMinRating.Contains(c.Id));
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(c => c.Price),
                "price_desc" => query.OrderByDescending(c => c.Price),
                "name_asc" => query.OrderBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                "rating" => query.OrderByDescending(c => 
                    comboRatings.ContainsKey(c.Id) ? comboRatings[c.Id].AvgRating : 0),
                _ => query.OrderByDescending(c => c.Id) // default: newest
            };
            
            var combos = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass data to view
            ViewBag.Keyword = keyword;
            ViewBag.SortBy = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinRating = minRating;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.ComboRatings = comboRatings;
            
            return View(combos);
        }

        public IActionResult DetailPopup(int id)
        {
            var combo = _context.Combos
                .Include(c => c.ComboDetails)
                    .ThenInclude(cd => cd.Food)
                .FirstOrDefault(c => c.Id == id && c.IsActive && !c.IsDeleted);
            if (combo == null) return NotFound();

            // Get reviews for this combo
            var reviews = _context.Reviews
                .Where(r => r.ComboId == id)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();
            
            var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            var reviewCount = _context.Reviews.Count(r => r.ComboId == id);
            
            ViewBag.Reviews = reviews;
            ViewBag.AvgRating = avgRating;
            ViewBag.ReviewCount = reviewCount;

            return PartialView("_ComboDetailPopup", combo);
        }
    }
}
