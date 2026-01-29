using ASM1_NET.Data;
using ASM1_NET.Models;
using ASM1_NET.ViewModels;
using ASM1_NET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ASM1_NET.Controllers;

/// <summary>
/// Controller xử lý đăng ký, đăng nhập, profile
/// Demo: Model Validation, Dependency Injection
/// </summary>
public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;  // ✅ Dependency Injection
    private readonly IActivityLogService _activityLog; // ✅ Activity Log

    // ✅ Constructor Injection
    public AccountController(AppDbContext context, IEmailService emailService, IActivityLogService activityLog)
    {
        _context = context;
        _emailService = emailService;
        _activityLog = activityLog;
    }

    // ================= PROFILE =================

    [HttpGet]
    public IActionResult Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = _context.Users.Find(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profile(string FullName, string Phone, string Address, string Password)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = _context.Users.Find(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        user.FullName = FullName;
        user.Phone = Phone;
        user.Address = Address;

        // Chỉ đổi mật khẩu nếu có nhập
        if (!string.IsNullOrEmpty(Password))
        {
            user.Password = Password;
        }

        _context.SaveChanges();

        ViewBag.Success = "Cập nhật thông tin thành công";
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAvatar(IFormFile avatar)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = _context.Users.Find(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        if (avatar != null && avatar.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"avatar_{userId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(avatar.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.AvatarUrl = $"/uploads/avatars/{fileName}";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật ảnh đại diện thành công!";
        }

        return RedirectToAction("Profile");
    }


    // ================= REGISTER với ViewModel & Model Validation =================
    
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());  // ✅ Trả về ViewModel
    }

    /// <summary>
    /// POST Register - Model Validation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // ✅ Model Validation - Kiểm tra ModelState
        if (!ModelState.IsValid)
        {
            return View(model);  // Trả về View với lỗi validation
        }

        // Business Validation - Email đã tồn tại
        if (_context.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng chọn email khác.");
            return View(model);
        }

        // Tạo User mới
        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            Phone = model.Phone,
            Address = model.Address,
            Role = "Customer",
            IsActive = true
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // ✅ Activity Log
        await _activityLog.LogAsync("Register", "User", user.Id, user.FullName, $"User mới đăng ký: {user.Email}");

        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }


    // ================= LOGIN với ViewModel & Model Validation =================
    
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());  // ✅ Trả về ViewModel
    }

    /// <summary>
    /// POST Login - Model Validation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // ✅ Model Validation
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == model.Email &&
                u.Password == model.Password &&
                u.IsActive &&
                !u.IsDeleted);  // ✅ Không cho user bị xóa đăng nhập

        if (user == null)
        {
            ModelState.AddModelError("", "Sai email hoặc mật khẩu");
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserName", user.FullName);

        // Tạo claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        // ✅ Activity Log
        await _activityLog.LogAsync("Login", "User", user.Id, user.FullName, $"{user.FullName} ({user.Role}) đã đăng nhập");

        // Phân luồng theo Role
        if (user.Role == "Admin")
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (user.Role == "Shipper")
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Shipper" });
        }

        // Customer
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        // ✅ Activity Log trước khi logout
        var userName = User.Identity?.Name ?? "Unknown";
        await _activityLog.LogWithUserAsync("Logout", "User", null, userName, $"{userName} đã đăng xuất");

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    // ================= FORGOT PASSWORD =================

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ViewBag.Error = "Vui lòng nhập email";
            return View();
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            ViewBag.Success = "Nếu email tồn tại, mã xác nhận đã được gửi đến hộp thư của bạn.";
            return View();
        }

        var code = new Random().Next(100000, 999999).ToString();

        var token = new PasswordResetToken
        {
            Email = email,
            Token = code,
            ExpiresAt = DateTime.Now.AddMinutes(15),
            IsUsed = false
        };
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        // ✅ Sử dụng injected service thay vì RequestServices
        await _emailService.SendPasswordResetCodeAsync(email, code);

        TempData["ResetEmail"] = email;
        return RedirectToAction("ResetPassword");
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        ViewBag.Email = TempData["ResetEmail"]?.ToString() ?? "";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string email, string code, string newPassword)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(newPassword))
        {
            ViewBag.Error = "Vui lòng điền đầy đủ thông tin";
            ViewBag.Email = email;
            return View();
        }

        var token = _context.PasswordResetTokens
            .Where(t => t.Email == email && t.Token == code && !t.IsUsed && t.ExpiresAt > DateTime.Now)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault();

        if (token == null)
        {
            ViewBag.Error = "Mã xác nhận không hợp lệ hoặc đã hết hạn";
            ViewBag.Email = email;
            return View();
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            user.Password = newPassword;
            token.IsUsed = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        ViewBag.Error = "Có lỗi xảy ra, vui lòng thử lại";
        return View();
    }

    // ================= GOOGLE OAUTH =================

    [HttpGet]
    public IActionResult ExternalLogin(string provider = "Google", string returnUrl = "/")
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
    {
        var authResult = await HttpContext.AuthenticateAsync("Google");
        if (!authResult.Succeeded)
        {
            TempData["Error"] = "Đăng nhập Google thất bại";
            return RedirectToAction("Login");
        }

        var claims = authResult.Principal?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var avatar = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Không thể lấy email từ Google";
            return RedirectToAction("Login");
        }

        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    FullName = name ?? email.Split('@')[0],
                    Password = Guid.NewGuid().ToString(),
                    Phone = "",
                    Address = "",
                    Role = "Customer",
                    IsActive = true,
                    AvatarUrl = avatar
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (!string.IsNullOrEmpty(avatar) && string.IsNullOrEmpty(user.AvatarUrl))
            {
                user.AvatarUrl = avatar;
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", user.FullName);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            if (user.Role == "Shipper")
                return RedirectToAction("Index", "Dashboard", new { area = "Shipper" });
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi lưu thông tin: {ex.Message}";
            return RedirectToAction("Login");
        }
    }
}