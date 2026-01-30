using System.Security.Claims;
using ASM1_NET.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Shipper")]
[Authorize(Roles = "Shipper")]
public class OrdersController : Controller
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult MyOrders()
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var orders = _context.Orders
            .Include(o => o.Customer)
            .Where(o => o.ShipperId == shipperId &&
                        !o.IsDeleted &&
                        (o.Status == "Delivering" || o.Status == "Completed"))
            .OrderByDescending(o => o.OrderDate)
            .ToList();
        
        ViewBag.DeliveringCount = orders.Count(o => o.Status == "Delivering");
        ViewBag.CompletedCount = orders.Count(o => o.Status == "Completed");
        ViewBag.TotalEarnings = orders.Where(o => o.Status == "Completed").Sum(o => o.TotalAmount);

        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id, IFormFile proofImage)
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var order = _context.Orders
            .FirstOrDefault(o => o.Id == id && o.ShipperId == shipperId && !o.IsDeleted);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng!";
            return RedirectToAction("MyOrders");
        }
        
        if (order.Status != "Delivering")
        {
            TempData["Error"] = "Đơn hàng không ở trạng thái đang giao!";
            return RedirectToAction("MyOrders");
        }

        if (proofImage == null || proofImage.Length == 0)
        {
            TempData["Error"] = "Vui lòng chụp/chọn ảnh xác nhận giao hàng!";
            return RedirectToAction("MyOrders");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "delivery-proofs");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{order.OrderCode}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(proofImage.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await proofImage.CopyToAsync(stream);
        }

        order.Status = "Completed";
        order.DeliveryProofImageUrl = $"/uploads/delivery-proofs/{fileName}";
        order.DeliveryDate = DateTime.Now;
        await _context.SaveChangesAsync();
        
        TempData["Success"] = $"Đã hoàn thành đơn #{order.OrderCode}! Ảnh xác nhận đã được lưu.";

        return RedirectToAction("MyOrders");
    }
    
    public IActionResult Detail(int id)
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var order = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Combo)
            .FirstOrDefault(o => o.Id == id && o.ShipperId == shipperId && !o.IsDeleted);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng!";
            return RedirectToAction("MyOrders");
        }

        return View(order);
    }
    
    public IActionResult History(string fromDate, string toDate)
    {
        var shipperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var query = _context.Orders
            .Include(o => o.Customer)
            .Where(o => o.ShipperId == shipperId && o.Status == "Completed" && !o.IsDeleted)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
        {
            query = query.Where(o => o.OrderDate >= from);
            ViewBag.FromDate = fromDate;
        }
        
        if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
        {
            query = query.Where(o => o.OrderDate <= to.AddDays(1));
            ViewBag.ToDate = toDate;
        }

        var orders = query.OrderByDescending(o => o.OrderDate).ToList();
        
        ViewBag.TotalOrders = orders.Count;
        ViewBag.TotalEarnings = orders.Sum(o => o.TotalAmount);

        return View(orders);
    }
}
