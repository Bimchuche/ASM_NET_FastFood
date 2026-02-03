using ASM1_NET.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class ChatController : Controller
{
    private readonly AppDbContext _context;

    public ChatController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sessions = await _context.ChatSessions
            .Include(s => s.Customer)
            .Include(s => s.Admin)
            .Include(s => s.Messages)
            .OrderByDescending(s => s.Status == "Open")
            .ThenByDescending(s => s.CreatedAt)
            .Take(50)
            .ToListAsync();

        return View(sessions);
    }

    public async Task<IActionResult> Session(int id)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Customer)
            .Include(s => s.Admin)
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            TempData["Error"] = "Không tìm thấy cuộc trò chuyện!";
            return RedirectToAction("Index");
        }

        // Mark messages as read
        var unreadMessages = session.Messages.Where(m => !m.IsRead && m.IsFromCustomer);
        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }
        await _context.SaveChangesAsync();

        return View(session);
    }

    [HttpPost]
    public async Task<IActionResult> CloseSession(int id)
    {
        var session = await _context.ChatSessions.FindAsync(id);
        if (session != null)
        {
            session.Status = "Closed";
            session.ClosedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã đóng cuộc trò chuyện!";
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _context.ChatMessages
            .Where(m => m.IsFromCustomer && !m.IsRead)
            .CountAsync();
        return Json(new { count });
    }
}
