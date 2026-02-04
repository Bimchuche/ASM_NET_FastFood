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

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IActivityLogService _activityLog;

    public AccountController(AppDbContext context, IEmailService emailService, IActivityLogService activityLog)
    {
        _context = context;
        _emailService = emailService;
        _activityLog = activityLog;
    }

    // Helper method để lấy UserId từ Claims hoặc Session
    private int? GetCurrentUserId()
    {
        // Ưu tiên Claims (cookie authentication)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int claimUserId))
        {
            // Đồng bộ lại Session nếu chưa có
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                var user = _context.Users.Find(claimUserId);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);
                }
            }
            return claimUserId;
        }
        
        // Fallback về Session
        return HttpContext.Session.GetInt32("UserId");
    }

    // Complete profile for Google login users
    [HttpGet]
    public IActionResult CompleteProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login");

        var user = _context.Users.Find(userId);
        if (user == null)
            return RedirectToAction("Login");

        // If profile already complete, redirect to home
        if (!string.IsNullOrEmpty(user.Phone) && !string.IsNullOrEmpty(user.Address))
            return RedirectToAction("Index", "Home");

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteProfile(string FullName, string Phone, string Address, string? Password)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return RedirectToAction("Login");

        // Validate
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Address))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
            return View(user);
        }

        user.FullName = FullName;
        user.Phone = Phone;
        user.Address = Address;
        
        // Set password if provided
        if (!string.IsNullOrWhiteSpace(Password))
        {
            user.Password = Password;
        }

        await _context.SaveChangesAsync();
        
        // Update session
        HttpContext.Session.SetString("UserName", user.FullName);

        TempData["Success"] = "Hoàn thiện thông tin thành công! Chào mừng bạn đến với FastFood.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Profile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = _context.Users.Find(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Load saved addresses for dropdown
        ViewBag.SavedAddresses = _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToList();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profile(string FullName, string Phone, string Address, string Password)
    {
        var userId = GetCurrentUserId();
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

        if (!string.IsNullOrEmpty(Password))
        {
            user.Password = Password;
        }

        _context.SaveChanges();

        // Reload saved addresses for dropdown
        ViewBag.SavedAddresses = _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToList();

        TempData["Success"] = "Cập nhật thông tin thành công!";
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAvatar(IFormFile avatar)
    {
        var userId = GetCurrentUserId();
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

    // Request password change - sends OTP to email
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestPasswordChange(string newPassword)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
            return RedirectToAction("Profile");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login");

        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();
        user.PasswordChangeOTP = otp;
        user.PasswordChangeOTPExpiry = DateTime.Now.AddMinutes(10);
        user.NewPasswordPending = newPassword;
        await _context.SaveChangesAsync();

        // Send OTP via email
        var emailBody = $@"
            <h2>Xác nhận đổi mật khẩu</h2>
            <p>Xin chào <strong>{user.FullName}</strong>,</p>
            <p>Bạn đã yêu cầu đổi mật khẩu. Mã xác thực của bạn là:</p>
            <div style='background: linear-gradient(135deg, #10b981, #059669); color: white; padding: 20px 40px; font-size: 32px; font-weight: bold; text-align: center; border-radius: 10px; margin: 20px 0; letter-spacing: 8px;'>
                {otp}
            </div>
            <p style='color: #888;'>Mã có hiệu lực trong 10 phút. Nếu không phải bạn yêu cầu, hãy bỏ qua email này.</p>
        ";
        await _emailService.SendEmailAsync(user.Email, "Mã xác thực đổi mật khẩu - FastFood", emailBody);

        TempData["ShowOTPModal"] = true;
        TempData["Success"] = "Mã xác thực đã được gửi đến email của bạn!";
        return RedirectToAction("Profile");
    }

    // Confirm password change with OTP
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPasswordChange(string otp)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login");

        if (string.IsNullOrEmpty(user.PasswordChangeOTP) || 
            user.PasswordChangeOTPExpiry < DateTime.Now ||
            user.PasswordChangeOTP != otp)
        {
            TempData["Error"] = "Mã xác thực không đúng hoặc đã hết hạn!";
            TempData["ShowOTPModal"] = true;
            return RedirectToAction("Profile");
        }

        // Update password
        user.Password = user.NewPasswordPending!;
        user.PasswordChangeOTP = null;
        user.PasswordChangeOTPExpiry = null;
        user.NewPasswordPending = null;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Profile");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate Gmail format
        if (!model.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Email", "Chỉ chấp nhận email Gmail (@gmail.com)");
            return View(model);
        }

        // Check if email exists (including deleted users)
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
            {
                ModelState.AddModelError("Email", "Email này thuộc tài khoản đã bị xóa. Vui lòng liên hệ Admin để khôi phục.");
            }
            else
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng chọn email khác.");
            }
            return View(model);
        }

        // Check duplicate phone number
        var existingPhone = await _context.Users.FirstOrDefaultAsync(u => u.Phone == model.Phone && !u.IsDeleted);
        if (existingPhone != null)
        {
            ModelState.AddModelError("Phone", "Số điện thoại đã được sử dụng. Vui lòng dùng số khác.");
            return View(model);
        }

        // Generate verification token
        var verificationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            Phone = model.Phone,
            Address = model.Address,
            Role = "Customer",
            IsActive = true,
            EmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.Now.AddHours(24)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send verification email with try-catch
        try
        {
            var verifyUrl = Url.Action("VerifyEmail", "Account", new { token = verificationToken }, Request.Scheme);
            var emailBody = $@"
                <h2>Xác thực email của bạn</h2>
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại FastFood Shop!</p>
                <p>Vui lòng nhấn vào nút bên dưới để xác thực email:</p>
                <p style='margin: 30px 0;'>
                    <a href='{verifyUrl}' style='background: linear-gradient(135deg, #10b981, #059669); color: white; padding: 15px 30px; text-decoration: none; border-radius: 10px; font-weight: bold;'>
                        ✅ Xác thực Email
                    </a>
                </p>
                <p>Hoặc copy link này vào trình duyệt:</p>
                <p style='word-break: break-all; color: #3b82f6;'>{verifyUrl}</p>
                <p style='color: #888;'>Link sẽ hết hạn sau 24 giờ.</p>
            ";
            await _emailService.SendEmailAsync(user.Email, "Xác thực email - FastFood Shop", emailBody);
            TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            // Still allow registration but notify user
            TempData["Success"] = "Đăng ký thành công! Tuy nhiên không gửi được email xác thực. Vui lòng liên hệ Admin.";
        }

        await _activityLog.LogAsync("Register", "User", user.Id, user.FullName, $"User mới đăng ký: {user.Email}");

        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Link xác thực không hợp lệ!";
            return RedirectToAction("Login");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.EmailVerificationToken == token && 
            u.EmailVerificationTokenExpiry > DateTime.Now);

        if (user == null)
        {
            TempData["Error"] = "Link xác thực không hợp lệ hoặc đã hết hạn!";
            return RedirectToAction("Login");
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if user exists with email
        var userCheck = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        
        if (userCheck == null)
        {
            ModelState.AddModelError("", "Sai email hoặc mật khẩu");
            return View(model);
        }
        
        if (userCheck.IsDeleted)
        {
            ModelState.AddModelError("", "Tài khoản này đã bị xóa. Vui lòng liên hệ Admin để khôi phục.");
            return View(model);
        }
        
        if (!userCheck.IsActive)
        {
            ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
            return View(model);
        }
        
        if (userCheck.Password != model.Password)
        {
            ModelState.AddModelError("", "Sai email hoặc mật khẩu");
            return View(model);
        }

        // Check email verification
        if (!userCheck.EmailVerified)
        {
            ModelState.AddModelError("", "Vui lòng xác thực email trước khi đăng nhập. Kiểm tra hộp thư của bạn.");
            return View(model);
        }

        var user = userCheck;

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserName", user.FullName);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            }
        );

        await _activityLog.LogAsync("Login", "User", user.Id, user.FullName, $"{user.FullName} ({user.Role}) đã đăng nhập");

        if (user.Role == "Admin")
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (user.Role == "Shipper")
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Shipper" });
        }

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name ?? "Unknown";
        await _activityLog.LogWithUserAsync("Logout", "User", null, userName, $"{userName} đã đăng xuất");

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

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
            bool isNewUser = false;
            
            if (user != null)
            {
                // Check if user is deleted
                if (user.IsDeleted)
                {
                    TempData["Error"] = "Tài khoản này đã bị xóa. Vui lòng liên hệ Admin để khôi phục.";
                    return RedirectToAction("Login");
                }
                
                // Check if user is inactive
                if (!user.IsActive)
                {
                    TempData["Error"] = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.";
                    return RedirectToAction("Login");
                }
                
                // Update avatar if not set
                if (!string.IsNullOrEmpty(avatar) && string.IsNullOrEmpty(user.AvatarUrl))
                {
                    user.AvatarUrl = avatar;
                    await _context.SaveChangesAsync();
                }
                
                // Check if profile is incomplete (no password means Google-only user who hasn't set info)
                if (string.IsNullOrEmpty(user.Phone) || string.IsNullOrEmpty(user.Address))
                {
                    isNewUser = true;
                }
            }
            else
            {
                // Create new user with empty password (will be set in CompleteProfile)
                user = new User
                {
                    Email = email,
                    FullName = name ?? email.Split('@')[0],
                    Password = "", // Empty for Google users
                    Phone = "",
                    Address = "",
                    Role = "Customer",
                    IsActive = true,
                    AvatarUrl = avatar
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                isNewUser = true;
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

            // Redirect new users to complete their profile
            if (isNewUser)
            {
                TempData["Info"] = "Vui lòng hoàn thiện thông tin tài khoản của bạn!";
                return RedirectToAction("CompleteProfile");
            }

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