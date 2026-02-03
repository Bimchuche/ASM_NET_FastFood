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
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLog;

        public UserController(AppDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                Phone = model.Phone,
                Address = model.Address,
                Role = model.Role,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model, string? NewPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.Role = model.Role;
            user.Email = model.Email;
            user.IsActive = model.IsActive;
            
            // Only update password if new password is provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                user.Password = NewPassword;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            if (user.Role == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin";
                return RedirectToAction(nameof(Index));
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "User", user.Id, user.FullName, $"Chuyển user vào thùng rác: {user.FullName}");

            TempData["Success"] = $"Đã chuyển '{user.FullName}' vào thùng rác!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 người dùng";
                return RedirectToAction(nameof(Index));
            }

            var users = await _context.Users.Where(u => ids.Contains(u.Id) && u.Role != "Admin").ToListAsync();
            var adminCount = ids.Length - users.Count;
            
            foreach (var user in users)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            
            await _activityLog.LogWithUserAsync("SoftDelete", "User", null, null, $"Xóa hàng loạt {users.Count} người dùng");
            
            if (adminCount > 0)
                TempData["Error"] = $"Không thể xóa {adminCount} tài khoản Admin";
            if (users.Count > 0)
                TempData["Success"] = $"Đã chuyển {users.Count} người dùng vào thùng rác";
            return RedirectToAction(nameof(Index));
        }
    }
}
