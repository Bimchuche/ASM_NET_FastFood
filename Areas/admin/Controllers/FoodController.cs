using ASM1_NET.Models;
using ASM1_NET.Repositories;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ASM1_NET.Data;

namespace ASM1_NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FoodController : Controller
    {
        private readonly IFoodRepository _foodRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IActivityLogService _activityLog;

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

        public async Task<IActionResult> Index()
        {
            var foods = await _foodRepository.Query()
                .Where(f => !f.IsDeleted)
                .Include(f => f.Category)
                .AsNoTracking()
                .OrderBy(f => f.Name)
                .ToListAsync();

            return View(foods);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetActiveAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food model, IFormFile? imageFile)
        {
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetActiveAsync();
                return View(model);
            }

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

            await _foodRepository.AddAsync(model);

            await _activityLog.LogWithUserAsync("Create", "Food", model.Id, model.Name, $"Tạo món ăn mới: {model.Name}");

            TempData["Success"] = "Thêm món ăn thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var food = await _foodRepository.GetWithCategoryAsync(id);
            if (food == null) return NotFound();

            ViewBag.Categories = await _categoryRepository.GetActiveAsync();
            return View(food);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Food model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetActiveAsync();
                return View(model);
            }

            var food = await _foodRepository.GetByIdAsync(id);
            if (food == null) return NotFound();

            food.Name = model.Name;
            food.Price = model.Price;
            food.Description = model.Description;
            food.CategoryId = model.CategoryId;
            food.IsAvailable = model.IsAvailable;

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

            await _foodRepository.UpdateAsync(food);

            await _activityLog.LogWithUserAsync("Update", "Food", food.Id, food.Name, $"Cập nhật món ăn: {food.Name}");

            TempData["Success"] = "Cập nhật món ăn thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var food = await _foodRepository.GetByIdAsync(id);
            if (food == null) return NotFound();

            food.IsDeleted = true;
            food.DeletedAt = DateTime.Now;
            await _foodRepository.UpdateAsync(food);

            await _activityLog.LogWithUserAsync("SoftDelete", "Food", food.Id, food.Name, $"Chuyển món ăn vào thùng rác: {food.Name}");

            TempData["Success"] = $"Đã chuyển '{food.Name}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            var foods = await _foodRepository.SearchAsync(keyword);
            
            ViewBag.Keyword = keyword;
            return View("Index", foods);
        }
    }
}
