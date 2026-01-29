using ASM1_NET.Models;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Order Repository Interface - Stored Procedures & Complex Queries
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>
        /// Execute Stored Procedure - Thống kê đơn hàng
        /// </summary>
        Task<OrderStatistics> GetStatisticsAsync(DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Lấy đơn hàng theo Customer - Eager Loading
        /// </summary>
        Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);

        /// <summary>
        /// Lấy đơn hàng theo Status - LINQ to Entities
        /// </summary>
        Task<IEnumerable<Order>> GetByStatusAsync(string status);

        /// <summary>
        /// Lấy chi tiết Order - Multiple Eager Loading
        /// </summary>
        Task<Order?> GetWithDetailsAsync(int orderId);

        /// <summary>
        /// Lấy đơn hàng của Shipper
        /// </summary>
        Task<IEnumerable<Order>> GetByShipperAsync(int shipperId);
    }

    /// <summary>
    /// DTO cho thống kê - Execute SQL
    /// </summary>
    public class OrderStatistics
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}
