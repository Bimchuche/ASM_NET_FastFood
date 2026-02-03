using ASM1_NET.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers;

[Area("Admin")]
public class ExportController : Controller
{
    private readonly AppDbContext _context;

    public ExportController(AppDbContext context)
    {
        _context = context;
    }

    // Export Dashboard
    public IActionResult Index()
    {
        return View();
    }

    // Export Orders to Excel
    [HttpGet]
    public async Task<IActionResult> Orders(DateTime? fromDate, DateTime? toDate, string? status)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
            .Where(o => !o.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value.AddDays(1));
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Đơn hàng");

        // Header
        worksheet.Cell(1, 1).Value = "Mã ĐH";
        worksheet.Cell(1, 2).Value = "Ngày đặt";
        worksheet.Cell(1, 3).Value = "Khách hàng";
        worksheet.Cell(1, 4).Value = "SĐT";
        worksheet.Cell(1, 5).Value = "Địa chỉ";
        worksheet.Cell(1, 6).Value = "Trạng thái";
        worksheet.Cell(1, 7).Value = "Thanh toán";
        worksheet.Cell(1, 8).Value = "Số món";
        worksheet.Cell(1, 9).Value = "Tổng tiền";

        // Style header
        var headerRange = worksheet.Range("A1:I1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.OrangeRed;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data
        for (int i = 0; i < orders.Count; i++)
        {
            var o = orders[i];
            worksheet.Cell(i + 2, 1).Value = $"#{o.Id}";
            worksheet.Cell(i + 2, 2).Value = o.OrderDate.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(i + 2, 3).Value = o.Customer?.FullName ?? "N/A";
            worksheet.Cell(i + 2, 4).Value = o.Phone;
            worksheet.Cell(i + 2, 5).Value = o.Address;
            worksheet.Cell(i + 2, 6).Value = o.Status;
            worksheet.Cell(i + 2, 7).Value = o.PaymentMethod;
            worksheet.Cell(i + 2, 8).Value = o.OrderDetails?.Count ?? 0;
            worksheet.Cell(i + 2, 9).Value = o.TotalAmount;
        }

        // Format currency
        worksheet.Column(9).Style.NumberFormat.Format = "#,##0 ₫";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Summary row
        var lastRow = orders.Count + 2;
        worksheet.Cell(lastRow, 8).Value = "Tổng cộng:";
        worksheet.Cell(lastRow, 8).Style.Font.Bold = true;
        worksheet.Cell(lastRow, 9).Value = orders.Sum(o => o.TotalAmount);
        worksheet.Cell(lastRow, 9).Style.Font.Bold = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"DonHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // Export Revenue Report to Excel
    [HttpGet]
    public async Task<IActionResult> Revenue(DateTime? fromDate, DateTime? toDate)
    {
        fromDate ??= DateTime.Today.AddMonths(-1);
        toDate ??= DateTime.Today;

        var orders = await _context.Orders
            .Where(o => !o.IsDeleted && o.Status == "Completed" &&
                        o.OrderDate.Date >= fromDate.Value && o.OrderDate.Date <= toDate.Value)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();

        // Group by date
        var dailyRevenue = orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Doanh thu");

        // Title
        worksheet.Cell(1, 1).Value = $"BÁO CÁO DOANH THU";
        worksheet.Range("A1:C1").Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        
        worksheet.Cell(2, 1).Value = $"Từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}";
        worksheet.Range("A2:C2").Merge();
        worksheet.Cell(2, 1).Style.Font.Italic = true;

        // Header
        worksheet.Cell(4, 1).Value = "Ngày";
        worksheet.Cell(4, 2).Value = "Số đơn";
        worksheet.Cell(4, 3).Value = "Doanh thu";

        var headerRange = worksheet.Range("A4:C4");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.Green;
        headerRange.Style.Font.FontColor = XLColor.White;

        // Data
        for (int i = 0; i < dailyRevenue.Count; i++)
        {
            var d = dailyRevenue[i];
            worksheet.Cell(i + 5, 1).Value = d.Date.ToString("dd/MM/yyyy");
            worksheet.Cell(i + 5, 2).Value = d.OrderCount;
            worksheet.Cell(i + 5, 3).Value = d.Revenue;
        }

        worksheet.Column(3).Style.NumberFormat.Format = "#,##0 ₫";

        // Summary
        var lastRow = dailyRevenue.Count + 6;
        worksheet.Cell(lastRow, 1).Value = "TỔNG CỘNG";
        worksheet.Cell(lastRow, 1).Style.Font.Bold = true;
        worksheet.Cell(lastRow, 2).Value = dailyRevenue.Sum(d => d.OrderCount);
        worksheet.Cell(lastRow, 2).Style.Font.Bold = true;
        worksheet.Cell(lastRow, 3).Value = dailyRevenue.Sum(d => d.Revenue);
        worksheet.Cell(lastRow, 3).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"DoanhThu_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // Export Products to Excel
    [HttpGet]
    public async Task<IActionResult> Products()
    {
        var foods = await _context.Foods
            .Include(f => f.Category)
            .Include(f => f.Reviews)
            .Where(f => !f.IsDeleted)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sản phẩm");

        // Header
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Tên món";
        worksheet.Cell(1, 3).Value = "Danh mục";
        worksheet.Cell(1, 4).Value = "Giá";
        worksheet.Cell(1, 5).Value = "Trạng thái";
        worksheet.Cell(1, 6).Value = "Đánh giá TB";
        worksheet.Cell(1, 7).Value = "Số review";

        var headerRange = worksheet.Range("A1:G1");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.RoyalBlue;
        headerRange.Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < foods.Count; i++)
        {
            var f = foods[i];
            var avgRating = f.Reviews?.Any() == true ? f.Reviews.Average(r => r.Rating) : 0;
            
            worksheet.Cell(i + 2, 1).Value = f.Id;
            worksheet.Cell(i + 2, 2).Value = f.Name;
            worksheet.Cell(i + 2, 3).Value = f.Category?.Name ?? "N/A";
            worksheet.Cell(i + 2, 4).Value = f.Price;
            worksheet.Cell(i + 2, 5).Value = f.IsAvailable ? "Còn hàng" : "Hết hàng";
            worksheet.Cell(i + 2, 6).Value = avgRating;
            worksheet.Cell(i + 2, 7).Value = f.Reviews?.Count ?? 0;
        }

        worksheet.Column(4).Style.NumberFormat.Format = "#,##0";
        worksheet.Columns().AdjustToContents();
        // Ensure price column is wide enough
        if (worksheet.Column(4).Width < 15)
            worksheet.Column(4).Width = 15;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"SanPham_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
