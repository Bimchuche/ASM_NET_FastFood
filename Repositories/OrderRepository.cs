using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<OrderStatistics> GetStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _dbSet.AsQueryable();
            
            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate);
            
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate);

            var orders = await query.ToListAsync();
            
            return new OrderStatistics
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                CancelledOrders = orders.Count(o => o.Status == "Cancelled")
            };
        }

        public async Task<OrderStatistics> GetStatisticsWithRawSqlAsync(DateTime? startDate, DateTime? endDate)
        {
            var sql = @"
                SELECT 
                    COUNT(*) AS TotalOrders,
                    ISNULL(SUM(TotalAmount), 0) AS TotalRevenue,
                    ISNULL(AVG(TotalAmount), 0) AS AverageOrderValue,
                    COUNT(CASE WHEN Status = 'Pending' THEN 1 END) AS PendingOrders,
                    COUNT(CASE WHEN Status = 'Delivered' THEN 1 END) AS DeliveredOrders,
                    COUNT(CASE WHEN Status = 'Cancelled' THEN 1 END) AS CancelledOrders
                FROM Orders
                WHERE (@p0 IS NULL OR OrderDate >= @p0)
                  AND (@p1 IS NULL OR OrderDate <= @p1)";

            var result = await _context.Database
                .SqlQueryRaw<OrderStatistics>(sql, startDate, endDate)
                .FirstOrDefaultAsync();

            return result ?? new OrderStatistics();
        }

        public async Task<IEnumerable<Order>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Combo)
                .Include(o => o.Shipper)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(o => o.Status == status)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order?> GetWithDetailsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                        .ThenInclude(f => f!.Category)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Combo)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetByShipperAsync(int shipperId)
        {
            return await _dbSet
                .Where(o => o.ShipperId == shipperId)
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task LoadOrderDetailsAsync(Order order)
        {
            await _context.Entry(order)
                .Reference(o => o.Customer)
                .LoadAsync();

            await _context.Entry(order)
                .Collection(o => o.OrderDetails)
                .LoadAsync();

            foreach (var detail in order.OrderDetails)
            {
                await _context.Entry(detail)
                    .Reference(d => d.Food)
                    .LoadAsync();
            }
        }
    }
}
