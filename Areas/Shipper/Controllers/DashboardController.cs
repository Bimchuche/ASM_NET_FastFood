using System.Security.Claims;
using ASM1_NET.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Shipper")]
[Authorize(Roles = "Shipper")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    // 📌 ĐƠN ĐANG CHỜ
    public IActionResult Index()
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        // Check if shipper has any delivering orders
        var hasDeliveringOrder = _context.Orders
            .Any(o => o.ShipperId == shipperId && o.Status == "Delivering" && !o.IsDeleted);
        
        ViewBag.HasDeliveringOrder = hasDeliveringOrder;
        
        var orders = _context.Orders
            .Include(o => o.Customer)
            .Where(o => o.Status == "Pending" && o.ShipperId == null && !o.IsDeleted)  // ✅ Ẩn đơn đã xóa
            .OrderBy(o => o.OrderDate)
            .ToList();

        return View(orders);
    }

    // 📌 NHẬN ĐƠN
    [HttpPost]
    public IActionResult Accept(int id, double? shipperLat, double? shipperLng)
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // ⚠️ KIỂM TRA: Shipper đang giao đơn khác chưa?
        var hasDeliveringOrder = _context.Orders
            .Any(o => o.ShipperId == shipperId && o.Status == "Delivering" && !o.IsDeleted);
        
        if (hasDeliveringOrder)
        {
            TempData["Error"] = "Bạn cần hoàn thành đơn hàng đang giao trước khi nhận đơn mới!";
            return RedirectToAction("Index");
        }

        var order = _context.Orders.FirstOrDefault(o => o.Id == id && !o.IsDeleted);

        if (order == null)
        {
            TempData["Error"] = "Đơn hàng không tồn tại!";
            return RedirectToAction("Index");
        }
        
        if (order.ShipperId != null)
        {
            TempData["Error"] = "Đơn hàng đã được shipper khác nhận!";
            return RedirectToAction("Index");
        }

        order.ShipperId = shipperId;
        order.Status = "Delivering";
        order.ConfirmedAt = DateTime.Now;
        
        // 🆕 Save shipper location & calculate estimated time
        if (shipperLat.HasValue && shipperLng.HasValue)
        {
            order.ShipperLatitude = shipperLat;
            order.ShipperLongitude = shipperLng;
            
            // Calculate distance if customer coordinates available
            if (order.DeliveryLatitude.HasValue && order.DeliveryLongitude.HasValue)
            {
                var distance = CalculateDistance(
                    shipperLat.Value, shipperLng.Value,
                    order.DeliveryLatitude.Value, order.DeliveryLongitude.Value
                );
                
                // 1 km = 3 minutes, minimum 5 minutes
                order.EstimatedMinutes = Math.Max(5, (int)Math.Ceiling(distance * 3));
            }
            else
            {
                // Default 15 minutes if no coordinates
                order.EstimatedMinutes = 15;
            }
        }
        else
        {
            // Default 15 minutes if no GPS
            order.EstimatedMinutes = 15;
        }

        _context.SaveChanges();
        
        TempData["Success"] = $"Đã nhận đơn #{order.OrderCode}! Thời gian giao dự kiến: {order.EstimatedMinutes} phút";

        // ✅ CHUYỂN QUA MY ORDERS NGAY
        return RedirectToAction("MyOrders", "Orders");
    }
    
    // 🆕 Haversine formula - calculate distance between 2 GPS points
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c; // Distance in km
    }
    
    private double ToRadians(double degrees) => degrees * Math.PI / 180;
    
    // 📌 HỦY NHẬN ĐƠN (Nếu chưa bắt đầu giao)
    [HttpPost]
    public IActionResult CancelAccept(int id)
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var order = _context.Orders
            .FirstOrDefault(o => o.Id == id && o.ShipperId == shipperId && o.Status == "Delivering" && !o.IsDeleted);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng!";
            return RedirectToAction("MyOrders", "Orders");
        }

        // Trả đơn hàng về trạng thái chờ
        order.ShipperId = null;
        order.Status = "Pending";
        
        _context.SaveChanges();
        
        TempData["Success"] = $"Đã hủy nhận đơn #{order.OrderCode}. Đơn hàng quay về danh sách chờ.";

        return RedirectToAction("Index");
    }
}
