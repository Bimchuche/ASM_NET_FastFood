using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Hubs;

public class ChatHub : Hub
{
    private readonly AppDbContext _context;

    public ChatHub(AppDbContext context)
    {
        _context = context;
    }

    // Customer sends message
    public async Task SendMessage(int sessionId, string message)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null || string.IsNullOrWhiteSpace(message)) return;

        var chatMessage = new ChatMessage
        {
            SessionId = sessionId,
            SenderId = session.CustomerId,
            Message = message.Trim(),
            IsFromCustomer = true
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        // Send to customer and admins
        await Clients.Group($"session_{sessionId}").SendAsync("ReceiveMessage", new
        {
            id = chatMessage.Id,
            message = chatMessage.Message,
            isFromCustomer = true,
            senderName = session.Customer?.FullName ?? "Khách",
            createdAt = chatMessage.CreatedAt.ToString("HH:mm")
        });

        // Notify admins of new message
        await Clients.Group("admins").SendAsync("NewCustomerMessage", new
        {
            sessionId = sessionId,
            customerName = session.Customer?.FullName ?? "Khách",
            message = chatMessage.Message
        });
    }

    // Admin sends message
    public async Task AdminSendMessage(int sessionId, string message, int adminId)
    {
        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session == null || session.Status != "Open" || string.IsNullOrWhiteSpace(message)) return;

        var admin = await _context.Users.FindAsync(adminId);

        var chatMessage = new ChatMessage
        {
            SessionId = sessionId,
            SenderId = adminId,
            Message = message.Trim(),
            IsFromCustomer = false
        };

        _context.ChatMessages.Add(chatMessage);

        // Assign admin to session if not assigned
        if (session.AdminId == null)
        {
            session.AdminId = adminId;
        }

        await _context.SaveChangesAsync();

        await Clients.Group($"session_{sessionId}").SendAsync("ReceiveMessage", new
        {
            id = chatMessage.Id,
            message = chatMessage.Message,
            isFromCustomer = false,
            senderName = admin?.FullName ?? "Hỗ trợ",
            createdAt = chatMessage.CreatedAt.ToString("HH:mm")
        });
    }

    // Join session group
    public async Task JoinSession(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    // Join admin group
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
    }

    // Start or resume chat session
    public async Task<object> StartSession(int customerId)
    {
        // Check if customer has open session
        var existingSession = await _context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(s => s.CustomerId == customerId && s.Status == "Open");

        if (existingSession != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{existingSession.Id}");
            
            // Return session with history
            return new
            {
                sessionId = existingSession.Id,
                isNew = false,
                messages = existingSession.Messages.Select(m => new
                {
                    id = m.Id,
                    message = m.Message,
                    isFromCustomer = m.IsFromCustomer,
                    senderName = m.Sender?.FullName ?? (m.IsFromCustomer ? "Bạn" : "Hỗ trợ"),
                    createdAt = m.CreatedAt.ToString("HH:mm")
                }).ToList()
            };
        }

        // Create new session
        var session = new ChatSession
        {
            CustomerId = customerId,
            Status = "Open"
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{session.Id}");

        // Notify admins
        var customer = await _context.Users.FindAsync(customerId);
        await Clients.Group("admins").SendAsync("NewChatSession", new
        {
            sessionId = session.Id,
            customerName = customer?.FullName ?? "Khách",
            createdAt = session.CreatedAt.ToString("dd/MM HH:mm")
        });

        return new
        {
            sessionId = session.Id,
            isNew = true,
            messages = new List<object>()
        };
    }

    // Close session
    public async Task CloseSession(int sessionId)
    {
        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = "Closed";
            session.ClosedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await Clients.Group($"session_{sessionId}").SendAsync("SessionClosed");
        }
    }
}
