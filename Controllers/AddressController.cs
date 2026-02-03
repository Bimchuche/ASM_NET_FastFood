using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Controllers;

public class AddressController : Controller
{
    private readonly AppDbContext _context;

    public AddressController(AppDbContext context)
    {
        _context = context;
    }

    // GET: List addresses
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Account");

        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        return View(addresses);
    }

    // POST: Add Address (AJAX)
    [HttpPost]
    public async Task<IActionResult> Add(string name, string fullAddress, string? phone, bool isDefault)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(fullAddress))
            return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin!" });

        // If set as default, unset other defaults
        if (isDefault)
        {
            var existingDefaults = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var a in existingDefaults) a.IsDefault = false;
        }

        var address = new UserAddress
        {
            UserId = userId.Value,
            Name = name.Trim(),
            FullAddress = fullAddress.Trim(),
            Phone = phone?.Trim(),
            IsDefault = isDefault
        };

        _context.UserAddresses.Add(address);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã thêm địa chỉ mới!", id = address.Id });
    }

    // POST: Update Address (AJAX)
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, string fullAddress, string? phone, bool isDefault)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
            return Json(new { success = false, message = "Không tìm thấy địa chỉ!" });

        address.Name = name.Trim();
        address.FullAddress = fullAddress.Trim();
        address.Phone = phone?.Trim();

        if (isDefault && !address.IsDefault)
        {
            var others = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault && a.Id != id)
                .ToListAsync();
            foreach (var a in others) a.IsDefault = false;
        }
        address.IsDefault = isDefault;

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã cập nhật địa chỉ!" });
    }

    // POST: Delete Address (AJAX)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
            return Json(new { success = false, message = "Không tìm thấy địa chỉ!" });

        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã xóa địa chỉ!" });
    }

    // POST: Set Default (AJAX)
    [HttpPost]
    public async Task<IActionResult> SetDefault(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .ToListAsync();

        foreach (var a in addresses)
        {
            a.IsDefault = (a.Id == id);
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Đã đặt làm mặc định!" });
    }

    // GET: Get addresses for checkout dropdown (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Json(new { success = false, addresses = new List<object>() });

        var addresses = await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.FullAddress,
                a.Phone,
                a.IsDefault
            })
            .ToListAsync();

        return Json(new { success = true, addresses = addresses });
    }
}
