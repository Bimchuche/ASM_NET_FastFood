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
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLog;

        public CategoryController(AppDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)  // ✅ Filter soft deleted
                .Include(c => c.Foods)
                .ToList();

            return View(categories);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Description ??= "";

            _context.Categories.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category model)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description ?? "";
            category.IsActive = model.IsActive;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE (SOFT DELETE) =================
        public async Task<IActionResult> Delete(int id)
        {
            var category = _context.Categories
                .Include(c => c.Foods)
                .FirstOrDefault(c => c.Id == id);

            if (category == null) return NotFound();

            // ✅ SOFT DELETE - Chuyển vào thùng rác
            category.IsDeleted = true;
            category.DeletedAt = DateTime.Now;
            _context.SaveChanges();

            await _activityLog.LogWithUserAsync("SoftDelete", "Category", category.Id, category.Name, $"Chuyển danh mục vào thùng rác: {category.Name}");

            TempData["Success"] = $"Đã chuyển '{category.Name}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }
    }
}
