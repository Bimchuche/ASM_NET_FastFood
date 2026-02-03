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
    public class CouponController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLog;

        public CouponController(AppDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            // Check if code already exists
            var exists = await _context.Coupons.AnyAsync(c => c.Code == coupon.Code && !c.IsDeleted);
            if (exists)
            {
                ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại!");
                return View(coupon);
            }

            coupon.Code = coupon.Code.ToUpper();
            coupon.CreatedAt = DateTime.Now;
            coupon.UsedCount = 0;

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("Create", "Coupon", coupon.Id, coupon.Code,
                $"Tạo mã giảm giá {coupon.Code} - Giảm {coupon.DiscountPercent}%");

            TempData["Success"] = $"Đã tạo mã giảm giá {coupon.Code}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null || coupon.IsDeleted)
            {
                TempData["Error"] = "Không tìm thấy mã giảm giá!";
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            var existingCoupon = await _context.Coupons.FindAsync(id);
            if (existingCoupon == null || existingCoupon.IsDeleted)
            {
                TempData["Error"] = "Không tìm thấy mã giảm giá!";
                return RedirectToAction(nameof(Index));
            }

            // Check if code already exists (exclude current)
            var codeExists = await _context.Coupons
                .AnyAsync(c => c.Code == coupon.Code && c.Id != id && !c.IsDeleted);
            if (codeExists)
            {
                ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại!");
                return View(coupon);
            }

            existingCoupon.Code = coupon.Code.ToUpper();
            existingCoupon.DiscountPercent = coupon.DiscountPercent;
            existingCoupon.MinOrderAmount = coupon.MinOrderAmount;
            existingCoupon.MaxDiscountAmount = coupon.MaxDiscountAmount;
            existingCoupon.ExpiryDate = coupon.ExpiryDate;
            existingCoupon.UsageLimit = coupon.UsageLimit;
            existingCoupon.IsActive = coupon.IsActive;

            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("Update", "Coupon", existingCoupon.Id, existingCoupon.Code,
                $"Cập nhật mã giảm giá {existingCoupon.Code}");

            TempData["Success"] = $"Đã cập nhật mã giảm giá {existingCoupon.Code}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["Error"] = "Không tìm thấy mã giảm giá!";
                return RedirectToAction(nameof(Index));
            }

            coupon.IsDeleted = true;
            await _context.SaveChangesAsync();

            await _activityLog.LogWithUserAsync("SoftDelete", "Coupon", coupon.Id, coupon.Code,
                $"Xóa mã giảm giá {coupon.Code}");

            TempData["Success"] = $"Đã xóa mã giảm giá {coupon.Code}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["Error"] = "Không tìm thấy mã giảm giá!";
                return RedirectToAction(nameof(Index));
            }

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            var status = coupon.IsActive ? "kích hoạt" : "tắt";
            TempData["Success"] = $"Đã {status} mã giảm giá {coupon.Code}";
            return RedirectToAction(nameof(Index));
        }
    }
}
