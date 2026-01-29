using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Services
{
    /// <summary>
    /// Service ghi và truy vấn Activity Log
    /// </summary>
    public class ActivityLogService : IActivityLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Ghi log hoạt động (không có user info)
        /// </summary>
        public async Task LogAsync(string action, string? entityType, int? entityId, string? entityName, string description)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                
                var log = new ActivityLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString()
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"[ActivityLog Error] {ex.Message}");
                Console.WriteLine($"[ActivityLog Error] Inner: {ex.InnerException?.Message}");
                Console.WriteLine($"[ActivityLog Error] Stack: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Ghi log với thông tin user từ HttpContext
        /// </summary>
        public async Task LogWithUserAsync(string action, string? entityType, int? entityId, string? entityName, string description)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                int? userId = null;
                string? userName = null;
                string? userRole = null;

                if (user?.Identity?.IsAuthenticated == true)
                {
                    // Lấy UserId từ Claims - sử dụng ClaimTypes.NameIdentifier
                    var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int parsedId))
                    {
                        userId = parsedId;
                    }

                    userName = user.Identity.Name;
                    userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                }

                var log = new ActivityLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName,
                    Description = description,
                    UserId = userId,
                    UserName = userName,
                    UserRole = userRole,
                    CreatedAt = DateTime.Now,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString()
                };

                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                Console.WriteLine($"[ActivityLog Error] {ex.Message}");
                Console.WriteLine($"[ActivityLog Error] Inner: {ex.InnerException?.Message}");
                Console.WriteLine($"[ActivityLog Error] Stack: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Lấy danh sách log gần đây
        /// </summary>
        public async Task<List<ActivityLog>> GetRecentLogsAsync(int count = 50)
        {
            try
            {
                Console.WriteLine($"[GetRecentLogsAsync] Fetching {count} logs...");
                var logs = await _context.ActivityLogs
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(count)
                    .AsNoTracking()
                    .ToListAsync();
                Console.WriteLine($"[GetRecentLogsAsync] Found {logs.Count} logs");
                return logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetRecentLogsAsync] Error: {ex.Message}");
                Console.WriteLine($"[GetRecentLogsAsync] Inner: {ex.InnerException?.Message}");
                return new List<ActivityLog>();
            }
        }

        /// <summary>
        /// Lấy log theo khoảng thời gian
        /// </summary>
        public async Task<List<ActivityLog>> GetLogsByDateAsync(DateTime from, DateTime to)
        {
            return await _context.ActivityLogs
                .Where(l => l.CreatedAt >= from && l.CreatedAt <= to)
                .OrderByDescending(l => l.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy log theo loại action
        /// </summary>
        public async Task<List<ActivityLog>> GetLogsByActionAsync(string action, int count = 50)
        {
            return await _context.ActivityLogs
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy log theo entity type
        /// </summary>
        public async Task<List<ActivityLog>> GetLogsByEntityAsync(string entityType, int count = 50)
        {
            return await _context.ActivityLogs
                .Where(l => l.EntityType == entityType)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Đếm số log theo ngày (7 ngày gần nhất)
        /// </summary>
        public async Task<Dictionary<string, int>> GetLogCountByDayAsync(int days = 7)
        {
            var fromDate = DateTime.Now.AddDays(-days).Date;
            
            var logs = await _context.ActivityLogs
                .Where(l => l.CreatedAt >= fromDate)
                .GroupBy(l => l.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i).Date;
                var count = logs.FirstOrDefault(l => l.Date == date)?.Count ?? 0;
                result.Add(date.ToString("dd/MM"), count);
            }

            return result;
        }
    }
}
