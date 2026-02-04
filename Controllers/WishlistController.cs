using ASM1_NET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM1_NET.Models;

namespace ASM1_NET.Controllers;

public class WishlistController : Controller
{
    private readonly AppDbContext _context;

    public WishlistController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Wishlist
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            ViewBag.RequireLogin = true;
            return View(new List<Wishlist>());
        }

        var wishlistItems = await _context.Wishlists
            .Include(w => w.Food)
            .ThenInclude(f => f!.Category)
            .Where(w => w.UserId == userId && !w.Food!.IsDeleted)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return View(wishlistItems);
    }

    // POST: Add to Wishlist (AJAX)
    [HttpPost]
    public async Task<IActionResult> Add(int foodId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        }

        // Check if already in wishlist
        var existing = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.FoodId == foodId);

        if (existing != null)
        {
            return Json(new { success = false, message = "Món ăn đã có trong danh sách yêu thích!" });
        }

        var wishlist = new Wishlist
        {
            UserId = userId.Value,
            FoodId = foodId
        };

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();

        var count = await _context.Wishlists.CountAsync(w => w.UserId == userId);

        return Json(new { success = true, message = "Đã thêm vào yêu thích!", count = count });
    }

    // POST: Remove from Wishlist (AJAX)
    [HttpPost]
    public async Task<IActionResult> Remove(int foodId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        }

        var wishlist = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.FoodId == foodId);

        if (wishlist == null)
        {
            return Json(new { success = false, message = "Không tìm thấy trong danh sách!" });
        }

        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();

        var count = await _context.Wishlists.CountAsync(w => w.UserId == userId);

        return Json(new { success = true, message = "Đã xóa khỏi yêu thích!", count = count });
    }

    // POST: Toggle Wishlist (AJAX)
    [HttpPost]
    public async Task<IActionResult> Toggle(int foodId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!", requireLogin = true });
        }

        var existing = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.FoodId == foodId);

        bool isAdded;
        string message;

        if (existing != null)
        {
            _context.Wishlists.Remove(existing);
            isAdded = false;
            message = "Đã xóa khỏi yêu thích!";
        }
        else
        {
            _context.Wishlists.Add(new Wishlist
            {
                UserId = userId.Value,
                FoodId = foodId
            });
            isAdded = true;
            message = "Đã thêm vào yêu thích!";
        }

        await _context.SaveChangesAsync();
        var count = await _context.Wishlists.CountAsync(w => w.UserId == userId);

        return Json(new { success = true, message = message, isAdded = isAdded, count = count });
    }

    // GET: Check if in wishlist (for page load)
    [HttpGet]
    public async Task<IActionResult> Check(int foodId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Json(new { inWishlist = false });
        }

        var exists = await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.FoodId == foodId);

        return Json(new { inWishlist = exists });
    }

    // GET: Wishlist count for header
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Json(new { count = 0 });
        }

        var count = await _context.Wishlists.CountAsync(w => w.UserId == userId);
        return Json(new { count = count });
    }
}
