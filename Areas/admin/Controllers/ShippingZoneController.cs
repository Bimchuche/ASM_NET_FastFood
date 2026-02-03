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
    public class ShippingZoneController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogService _activityLog;

        public ShippingZoneController(AppDbContext context, IActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public async Task<IActionResult> Index()
        {
            var zones = await _context.ShippingZones
                .OrderBy(z => z.MinDistance)
                .ToListAsync();
            return View(zones);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingZone zone)
        {
            if (ModelState.IsValid)
            {
                _context.ShippingZones.Add(zone);
                await _context.SaveChangesAsync();
                
                await _activityLog.LogWithUserAsync("Create", "ShippingZone", zone.Id, zone.Name,
                    $"Tạo khu vực giao hàng: {zone.Name} ({zone.MinDistance}-{zone.MaxDistance}km, {zone.ShippingFee:N0}đ)");
                
                TempData["Success"] = $"Đã tạo khu vực '{zone.Name}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(zone);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();
            return View(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShippingZone zone)
        {
            if (id != zone.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(zone);
                await _context.SaveChangesAsync();
                
                await _activityLog.LogWithUserAsync("Update", "ShippingZone", zone.Id, zone.Name,
                    $"Cập nhật khu vực: {zone.Name}");
                
                TempData["Success"] = $"Đã cập nhật '{zone.Name}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();

            _context.ShippingZones.Remove(zone);
            await _context.SaveChangesAsync();
            
            await _activityLog.LogWithUserAsync("Delete", "ShippingZone", id, zone.Name,
                $"Xóa khu vực giao hàng: {zone.Name}");
            
            TempData["Success"] = $"Đã xóa '{zone.Name}'!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return NotFound();

            zone.IsActive = !zone.IsActive;
            await _context.SaveChangesAsync();
            
            TempData["Success"] = zone.IsActive ? $"Đã kích hoạt '{zone.Name}'!" : $"Đã tắt '{zone.Name}'!";
            return RedirectToAction(nameof(Index));
        }
    }
}
