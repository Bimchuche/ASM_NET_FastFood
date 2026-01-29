using ASM1_NET.Models;
using ASM1_NET.Repositories;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ASM1_NET.Data;

namespace ASM1_NET.Areas.Admin.Controllers
{
    /// <summary>
    /// Food Controller - Admin Area
    /// Demo: Dependency Injection với Repository Pattern
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FoodController : Controller
    {
        private readonly IFoodRepository _foodRepository;        // ✅ DI
        private readonly ICategoryRepository _categoryRepository; // ✅ DI
        private readonly IWebHostEnvironment _env;
        private readonly IActivityLogService _activityLog;       // ✅ Activity Log

        // ✅ Constructor Injection
        public FoodController(
            IFoodRepository foodRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment env,
            IActivityLogService activityLog)
        {
            _foodRepository = foodRepository;
            _categoryRepository = categoryRepository;
            _env = env;
            _activityLog = activityLog;
        }

        // ================= INDEX - Eager Loading =================
        public async Task<IActionResult> Index()
        {
            // ✅ Eager Loading với Include - Lọc bỏ items đã xóa
            var foods = await _foodRepository.Query()
                .Where(f => !f.IsDeleted)  // ✅ Soft Delete filter
                .Include(f => f.Category)
                .AsNoTracking()  // Query Performance
                .OrderBy(f => f.Name)
                .ToListAsync();

            return View(foods);
        }

        // ================= CREATE =================
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetActiveAsync();
            return View();
        }

        /// <summary>
        /// POST Create - Model Validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food model, IFormFile? imageFile)
        {
            // ✅ Model Validation - Remove navigation property
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetActiveAsync();
                return View(model);
            }

            // Upload ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads/foods");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                model.ImageUrl = "/uploads/foods/" + fileName;
            }

            // ✅ Sử dụng Repository
            await _foodRepository.AddAsync(model);

            // ✅ Activity Log
            await _activityLog.LogWithUserAsync("Create", "Food", model.Id, model.Name, $"Tạo món ăn mới: {model.Name}");

            TempData["Success"] = "Thêm món ăn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT - Explicit Loading =================
        public async Task<IActionResult> Edit(int id)
        {
            // ✅ Explicit Loading qua Repository
            var food = await _foodRepository.GetWithCategoryAsync(id);
            if (food == null) return NotFound();

            ViewBag.Categories = await _categoryRepository.GetActiveAsync();
            return View(food);
        }

        /// <summary>
        /// POST Edit - Model Validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Food model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            // ✅ Model Validation
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetActiveAsync();
                return View(model);
            }

            var food = await _foodRepository.GetByIdAsync(id);
            if (food == null) return NotFound();

            // Cập nhật thông tin
            food.Name = model.Name;
            food.Price = model.Price;
            food.Description = model.Description;
            food.CategoryId = model.CategoryId;
            food.IsAvailable = model.IsAvailable;

            // Sửa ảnh nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads/foods");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                food.ImageUrl = "/uploads/foods/" + fileName;
            }

            // ✅ Sử dụng Repository
            await _foodRepository.UpdateAsync(food);

            // ✅ Activity Log
            await _activityLog.LogWithUserAsync("Update", "Food", food.Id, food.Name, $"Cập nhật món ăn: {food.Name}");

            TempData["Success"] = "Cập nhật món ăn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE (SOFT DELETE) =================
        public async Task<IActionResult> Delete(int id)
        {
            var food = await _foodRepository.GetByIdAsync(id);
            if (food == null) return NotFound();

            // ✅ SOFT DELETE - Chuyển vào thùng rác thay vì xóa vĩnh viễn
            food.IsDeleted = true;
            food.DeletedAt = DateTime.Now;
            await _foodRepository.UpdateAsync(food);

            // ✅ Activity Log
            await _activityLog.LogWithUserAsync("SoftDelete", "Food", food.Id, food.Name, $"Chuyển món ăn vào thùng rác: {food.Name}");

            TempData["Success"] = $"Đã chuyển '{food.Name}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }

        // ================= SEARCH - LINQ to Entities =================
        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            // ✅ LINQ to Entities qua Repository
            var foods = await _foodRepository.SearchAsync(keyword);
            
            ViewBag.Keyword = keyword;
            return View("Index", foods);
        }
    }
}
