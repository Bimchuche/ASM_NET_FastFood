using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM1_NET.Data;

namespace ASM1_NET.Controllers
{
    public class FoodController : Controller
    {
        private readonly AppDbContext _context;

        public FoodController(AppDbContext context)
        {
            _context = context;
        }

        // Guest xem được
        public IActionResult Index(string keyword, int? categoryId)
        {
            var query = _context.Foods
                .Include(f => f.Category)
                .Where(f => f.IsAvailable && !f.IsDeleted);  // ✅ Ẩn món đã xóa

            // Filter by category
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(f => f.CategoryId == categoryId.Value);
            }

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(f =>
                    f.Name.Contains(keyword) ||
                    (f.Description ?? "").Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive && !c.IsDeleted).ToList();  // ✅ Ẩn danh mục đã xóa

            return View(query.ToList());
        }
        public IActionResult DetailPopup(int id)
        {
            var food = _context.Foods.FirstOrDefault(f => f.Id == id && f.IsAvailable && !f.IsDeleted);
            if (food == null) return NotFound();

            return PartialView("_FoodDetailPopup", food);
        }

    }
}
