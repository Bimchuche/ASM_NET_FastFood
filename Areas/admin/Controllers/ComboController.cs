using ASM1_NET.Data;
using ASM1_NET.Models;
using ASM1_NET.ViewModels;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ComboController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IActivityLogService _activityLog;

        public ComboController(AppDbContext context, IWebHostEnvironment env, IActivityLogService activityLog)
        {
            _context = context;
            _env = env;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            var combos = _context.Combos.Where(c => !c.IsDeleted).ToList();
            return View(combos);
        }

        public IActionResult Create()
        {
            var viewModel = new CreateComboViewModel();
            
            var foods = _context.Foods.Include(f => f.Category).Where(f => f.IsAvailable).ToList();
            
            foreach (var food in foods)
            {
                var item = new FoodCheckboxItem
                {
                    Id = food.Id,
                    Name = food.Name,
                    Price = food.Price,
                    ImageUrl = food.ImageUrl,
                    CategoryName = food.Category?.Name ?? "Khác",
                    IsSelected = false,
                    Quantity = 1
                };

                var catName = (food.Category?.Name ?? "").ToLower();
                if (catName.Contains("uống") || catName.Contains("nước") || catName.Contains("drink"))
                {
                    viewModel.DrinkList.Add(item);
                }
                else
                {
                    viewModel.FoodList.Add(item);
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(CreateComboViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "Vui lòng nhập tên combo!";
                return RedirectToAction(nameof(Create));
            }

            var combo = new Combo
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description ?? "",
                IsActive = model.IsActive
            };

            if (model.ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                if (allowedExtensions.Contains(extension))
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "combos");
                    Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid() + extension;
                    string path = Path.Combine(folder, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    model.ImageFile.CopyTo(stream);
                    combo.ImageUrl = "/uploads/combos/" + fileName;
                }
            }

            _context.Combos.Add(combo);
            _context.SaveChanges();

            var allSelected = model.FoodList.Concat(model.DrinkList).Where(x => x.IsSelected).ToList();
            foreach (var item in allSelected)
            {
                if (item.Quantity < 1) item.Quantity = 1;
                
                var detail = new ComboDetail
                {
                    ComboId = combo.Id,
                    FoodId = item.Id,
                    Quantity = item.Quantity
                };
                _context.ComboDetails.Add(detail);
            }

            _context.SaveChanges();

            TempData["Success"] = "Đã tạo combo thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var combo = _context.Combos.Find(id);
            if (combo == null) return NotFound();

            var existingDetails = _context.ComboDetails
                .Where(cd => cd.ComboId == id)
                .ToList();

            var viewModel = new CreateComboViewModel
            {
                Name = combo.Name,
                Price = combo.Price,
                Description = combo.Description,
                IsActive = combo.IsActive
            };

            var foods = _context.Foods.Include(f => f.Category).Where(f => f.IsAvailable).ToList();
            
            foreach (var food in foods)
            {
                var existingDetail = existingDetails.FirstOrDefault(d => d.FoodId == food.Id);
                
                var item = new FoodCheckboxItem
                {
                    Id = food.Id,
                    Name = food.Name,
                    Price = food.Price,
                    ImageUrl = food.ImageUrl,
                    CategoryName = food.Category?.Name ?? "Khác",
                    IsSelected = existingDetail != null,
                    Quantity = existingDetail?.Quantity ?? 1
                };

                var catName = (food.Category?.Name ?? "").ToLower();
                if (catName.Contains("uống") || catName.Contains("nước") || catName.Contains("drink"))
                {
                    viewModel.DrinkList.Add(item);
                }
                else
                {
                    viewModel.FoodList.Add(item);
                }
            }

            ViewBag.ComboId = id;
            ViewBag.ExistingImageUrl = combo.ImageUrl;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(int id, CreateComboViewModel model)
        {
            var combo = _context.Combos.Find(id);
            if (combo == null) return NotFound();

            combo.Name = model.Name;
            combo.Price = model.Price;
            combo.Description = model.Description ?? "";
            combo.IsActive = model.IsActive;

            if (model.ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                if (allowedExtensions.Contains(extension))
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads", "combos");
                    Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid() + extension;
                    string path = Path.Combine(folder, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    model.ImageFile.CopyTo(stream);
                    combo.ImageUrl = "/uploads/combos/" + fileName;
                }
            }

            var oldDetails = _context.ComboDetails.Where(cd => cd.ComboId == id).ToList();
            _context.ComboDetails.RemoveRange(oldDetails);

            var allSelected = model.FoodList.Concat(model.DrinkList).Where(x => x.IsSelected).ToList();
            foreach (var item in allSelected)
            {
                if (item.Quantity < 1) item.Quantity = 1;
                _context.ComboDetails.Add(new ComboDetail
                {
                    ComboId = id,
                    FoodId = item.Id,
                    Quantity = item.Quantity
                });
            }

            _context.SaveChanges();
            TempData["Success"] = "Đã cập nhật combo thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var combo = _context.Combos.Find(id);
            if (combo == null) return NotFound();

            combo.IsDeleted = true;
            combo.DeletedAt = DateTime.Now;
            _context.SaveChanges();

            await _activityLog.LogWithUserAsync("SoftDelete", "Combo", combo.Id, combo.Name, $"Chuyển combo vào thùng rác: {combo.Name}");

            TempData["Success"] = $"Đã chuyển '{combo.Name}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }
    }
}
