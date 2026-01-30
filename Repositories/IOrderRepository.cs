using ASM1_NET.Models;

namespace ASM1_NET.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<OrderStatistics> GetStatisticsAsync(DateTime? startDate, DateTime? endDate);

        Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);

        Task<IEnumerable<Order>> GetByStatusAsync(string status);

        Task<Order?> GetWithDetailsAsync(int orderId);

        Task<IEnumerable<Order>> GetByShipperAsync(int shipperId);

        Task LoadOrderDetailsAsync(Order order);
    }

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
