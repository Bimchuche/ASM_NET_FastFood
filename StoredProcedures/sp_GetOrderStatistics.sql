-- ====================================================
-- Stored Procedure: sp_GetOrderStatistics
-- Mục đích: Thống kê đơn hàng theo khoảng thời gian
-- Sử dụng với EF Core: Execute SQL Stored Procedures
-- ====================================================

CREATE PROCEDURE sp_GetOrderStatistics
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS TotalOrders,
        ISNULL(SUM(TotalAmount), 0) AS TotalRevenue,
        ISNULL(AVG(TotalAmount), 0) AS AverageOrderValue,
        COUNT(CASE WHEN Status = 'Pending' THEN 1 END) AS PendingOrders,
        COUNT(CASE WHEN Status = 'Confirmed' THEN 1 END) AS ConfirmedOrders,
        COUNT(CASE WHEN Status = 'Shipping' THEN 1 END) AS ShippingOrders,
        COUNT(CASE WHEN Status = 'Delivered' THEN 1 END) AS DeliveredOrders,
        COUNT(CASE WHEN Status = 'Cancelled' THEN 1 END) AS CancelledOrders
    FROM Orders
    WHERE (@StartDate IS NULL OR OrderDate >= @StartDate)
      AND (@EndDate IS NULL OR OrderDate <= @EndDate);
END
GO

-- ====================================================
-- Stored Procedure: sp_GetTopSellingFoods
-- Mục đích: Lấy top N món ăn bán chạy nhất
-- ====================================================

CREATE PROCEDURE sp_GetTopSellingFoods
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopN)
        f.Id,
        f.Name,
        f.Price,
        f.ImageUrl,
        c.Name AS CategoryName,
        SUM(od.Quantity) AS TotalSold,
        SUM(od.Quantity * od.UnitPrice) AS TotalRevenue
    FROM Foods f
    INNER JOIN OrderDetails od ON f.Id = od.FoodId
    INNER JOIN Orders o ON od.OrderId = o.Id
    INNER JOIN Categories c ON f.CategoryId = c.Id
    WHERE o.Status = 'Delivered'
    GROUP BY f.Id, f.Name, f.Price, f.ImageUrl, c.Name
    ORDER BY TotalSold DESC;
END
GO

-- ====================================================
-- Stored Procedure: sp_GetCustomerOrderHistory
-- Mục đích: Lấy lịch sử đơn hàng của khách
-- ====================================================

CREATE PROCEDURE sp_GetCustomerOrderHistory
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.Id,
        o.OrderCode,
        o.OrderDate,
        o.Status,
        o.TotalAmount,
        o.Address,
        o.Phone,
        o.PaymentMethod,
        (SELECT COUNT(*) FROM OrderDetails WHERE OrderId = o.Id) AS ItemCount
    FROM Orders o
    WHERE o.CustomerId = @CustomerId
    ORDER BY o.OrderDate DESC;
END
GO

-- ====================================================
-- Hướng dẫn sử dụng trong EF Core:
-- ====================================================
-- 
-- // Cách 1: SqlQueryRaw (EF Core 8.0+)
-- var result = await _context.Database
--     .SqlQueryRaw<OrderStatistics>("EXEC sp_GetOrderStatistics @p0, @p1", startDate, endDate)
--     .ToListAsync();
--
-- // Cách 2: FromSqlRaw (với DbSet)
-- var foods = await _context.Foods
--     .FromSqlRaw("EXEC sp_GetTopSellingFoods @p0", topN)
--     .ToListAsync();
--
-- // Cách 3: ExecuteSqlRawAsync (cho INSERT/UPDATE/DELETE)
-- await _context.Database
--     .ExecuteSqlRawAsync("EXEC sp_UpdateOrderStatus @p0, @p1", orderId, newStatus);
-- ====================================================
