namespace ASM1_NET.Services
{
    public interface IActivityLogService
    {
        Task LogAsync(string action, string? entityType, int? entityId, string? entityName, string description);

        Task LogWithUserAsync(string action, string? entityType, int? entityId, string? entityName, string description);

        Task<List<Models.ActivityLog>> GetRecentLogsAsync(int count = 50);

        Task<List<Models.ActivityLog>> GetLogsByDateAsync(DateTime from, DateTime to);

        Task<List<Models.ActivityLog>> GetLogsByActionAsync(string action, int count = 50);

        Task<List<Models.ActivityLog>> GetLogsByEntityAsync(string entityType, int count = 50);

        Task<Dictionary<string, int>> GetLogCountByDayAsync(int days = 7);
    }
}
