using ASM1_NET.Services;
using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller quản lý Activity Log
    /// </summary>
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

        /// <summary>
        /// Test endpoint to check raw database data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestData()
        {
            try
            {
                // Test 1: EF Core query
                var efCount = await _context.ActivityLogs.CountAsync();
                
                // Test 2: Raw SQL - sử dụng ExecuteSqlRawAsync để kiểm tra
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM ActivityLogs";
                var rawCount = await command.ExecuteScalarAsync();
                
                // Test 3: Connection string
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

        /// <summary>
        /// Trang Activity Log chính
        /// </summary>
        public IActionResult Index(string? filterAction, string? entityType, DateTime? from, DateTime? to)
        {
            ViewData["Title"] = "Nhật ký hoạt động";
            
            // Lấy logs trực tiếp từ DbContext
            var logs = _context.ActivityLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(100)
                .AsNoTracking()
                .ToList();

            // Filter by action
            if (!string.IsNullOrEmpty(filterAction))
            {
                logs = logs.Where(l => l.Action == filterAction).ToList();
            }

            // Filter by entity type  
            if (!string.IsNullOrEmpty(entityType))
            {
                logs = logs.Where(l => l.EntityType == entityType).ToList();
            }

            // Filter by date range
            if (from.HasValue)
            {
                logs = logs.Where(l => l.CreatedAt.Date >= from.Value.Date).ToList();
            }
            if (to.HasValue)
            {
                logs = logs.Where(l => l.CreatedAt.Date <= to.Value.Date).ToList();
            }
            
            // Set ViewBag
            ViewBag.LogCountByDay = new Dictionary<string, int>();
            ViewBag.Actions = new[] { "Login", "Logout", "Create", "Update", "Delete", "SoftDelete", "Restore", "Order", "Cancel", "Register" };
            ViewBag.EntityTypes = new[] { "User", "Food", "Combo", "Category", "Order" };
            ViewBag.CurrentAction = filterAction;
            ViewBag.CurrentEntityType = entityType;
            ViewBag.CurrentFrom = from;
            ViewBag.CurrentTo = to;

            return View(logs);
        }

        /// <summary>
        /// API lấy log mới (cho AJAX refresh)
        /// </summary>
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

        /// <summary>
        /// Widget cho Dashboard (partial view)
        /// </summary>
        public async Task<IActionResult> Widget(int count = 5)
        {
            var logs = await _activityLogService.GetRecentLogsAsync(count);
            return PartialView("_ActivityWidget", logs);
        }
    }
}
