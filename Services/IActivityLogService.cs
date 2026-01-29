namespace ASM1_NET.Services
{
    /// <summary>
    /// Interface cho Activity Log Service
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Ghi log hoạt động
        /// </summary>
        Task LogAsync(string action, string? entityType, int? entityId, string? entityName, string description);

        /// <summary>
        /// Ghi log với thông tin user từ HttpContext
        /// </summary>
        Task LogWithUserAsync(string action, string? entityType, int? entityId, string? entityName, string description);

        /// <summary>
        /// Lấy danh sách log gần đây
        /// </summary>
        Task<List<Models.ActivityLog>> GetRecentLogsAsync(int count = 50);

        /// <summary>
        /// Lấy log theo khoảng thời gian
        /// </summary>
        Task<List<Models.ActivityLog>> GetLogsByDateAsync(DateTime from, DateTime to);

        /// <summary>
        /// Lấy log theo loại action
        /// </summary>
        Task<List<Models.ActivityLog>> GetLogsByActionAsync(string action, int count = 50);

        /// <summary>
        /// Lấy log theo entity type
        /// </summary>
        Task<List<Models.ActivityLog>> GetLogsByEntityAsync(string entityType, int count = 50);

        /// <summary>
        /// Đếm số log theo ngày
        /// </summary>
        Task<Dictionary<string, int>> GetLogCountByDayAsync(int days = 7);
    }
}
