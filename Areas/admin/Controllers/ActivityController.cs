using ASM1_NET.Services;
using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ActivityController : Controller
    {
        private readonly IActivityLogService _activityLogService;
        private readonly AppDbContext _context;

        public ActivityController(IActivityLogService activityLogService, AppDbContext context)
        {
            _activityLogService = activityLogService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> TestData()
        {
            try
            {
                var efCount = await _context.ActivityLogs.CountAsync();
                
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM ActivityLogs";
                var rawCount = await command.ExecuteScalarAsync();
                
                var connString = _context.Database.GetConnectionString();
                
                return Json(new { 
                    EFCoreCount = efCount,
                    RawSqlCount = rawCount,
                    ConnectionString = connString?.Substring(0, Math.Min(50, connString?.Length ?? 0)) + "..."
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, Inner = ex.InnerException?.Message });
            }
        }

        public IActionResult Index(string? filterAction, string? entityType, DateTime? from, DateTime? to)
        {
            ViewData["Title"] = "Nhật ký hoạt động";
            
            var logs = _context.ActivityLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(100)
                .AsNoTracking()
                .ToList();

            if (!string.IsNullOrEmpty(filterAction))
            {
                logs = logs.Where(l => l.Action == filterAction).ToList();
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                logs = logs.Where(l => l.EntityType == entityType).ToList();
            }

            if (from.HasValue)
            {
                logs = logs.Where(l => l.CreatedAt.Date >= from.Value.Date).ToList();
            }
            if (to.HasValue)
            {
                logs = logs.Where(l => l.CreatedAt.Date <= to.Value.Date).ToList();
            }
            
            ViewBag.LogCountByDay = new Dictionary<string, int>();
            ViewBag.Actions = new[] { "Login", "Logout", "Create", "Update", "Delete", "SoftDelete", "Restore", "Order", "Cancel", "Register" };
            ViewBag.EntityTypes = new[] { "User", "Food", "Combo", "Category", "Order" };
            ViewBag.CurrentAction = filterAction;
            ViewBag.CurrentEntityType = entityType;
            ViewBag.CurrentFrom = from;
            ViewBag.CurrentTo = to;

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 10)
        {
            var logs = await _activityLogService.GetRecentLogsAsync(count);
            return Json(logs.Select(l => new
            {
                l.Id,
                l.Action,
                l.EntityType,
                l.EntityId,
                l.EntityName,
                l.Description,
                l.UserName,
                l.UserRole,
                CreatedAt = l.CreatedAt.ToString("dd/MM HH:mm"),
                Icon = l.GetActionIcon(),
                BadgeClass = l.GetActionBadgeClass()
            }));
        }

        public async Task<IActionResult> Widget(int count = 5)
        {
            var logs = await _activityLogService.GetRecentLogsAsync(count);
            return PartialView("_ActivityWidget", logs);
        }
    }
}
