using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Services;

public interface ILoyaltyService
{
    Task<int> GetUserPoints(int userId);
    Task<bool> AddPoints(int userId, int points, string type, string description, int? orderId = null);
    Task<bool> RedeemPoints(int userId, int points, string description, int? orderId = null);
    Task<List<LoyaltyPoint>> GetUserHistory(int userId, int limit = 20);
    int CalculatePointsForOrder(decimal orderTotal);
}

public class LoyaltyService : ILoyaltyService
{
    private readonly AppDbContext _context;
    
    // Points settings - can be moved to config/admin settings
    private const int POINTS_PER_1000VND = 1; // 1 point per 1000 VND spent
    private const int POINTS_EXPIRY_DAYS = 365; // Points expire after 1 year
    
    public LoyaltyService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> GetUserPoints(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.TotalPoints ?? 0;
    }
    
    public int CalculatePointsForOrder(decimal orderTotal)
    {
        // 1 point per 1000 VND
        return (int)(orderTotal / 1000) * POINTS_PER_1000VND;
    }
    
    public async Task<bool> AddPoints(int userId, int points, string type, string description, int? orderId = null)
    {
        if (points <= 0) return false;
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        
        // Create point transaction
        var loyaltyPoint = new LoyaltyPoint
        {
            UserId = userId,
            Points = points,
            Type = type,
            Description = description,
            OrderId = orderId,
            ExpiresAt = DateTime.Now.AddDays(POINTS_EXPIRY_DAYS)
        };
        
        _context.LoyaltyPoints.Add(loyaltyPoint);
        
        // Update user total
        user.TotalPoints += points;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> RedeemPoints(int userId, int points, string description, int? orderId = null)
    {
        if (points <= 0) return false;
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.TotalPoints < points) return false;
        
        // Create redemption transaction (negative points)
        var loyaltyPoint = new LoyaltyPoint
        {
            UserId = userId,
            Points = -points,
            Type = "Redeem",
            Description = description,
            OrderId = orderId
        };
        
        _context.LoyaltyPoints.Add(loyaltyPoint);
        
        // Update user total
        user.TotalPoints -= points;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<LoyaltyPoint>> GetUserHistory(int userId, int limit = 20)
    {
        return await _context.LoyaltyPoints
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Include(p => p.Order)
            .ToListAsync();
    }
}
