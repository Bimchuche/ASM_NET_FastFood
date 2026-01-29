using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ASM1_NET.Data;
using ASM1_NET.Models;

namespace ASM1_NET.ViewComponents
{
    public class MiniCartViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public MiniCartViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Try Session first (set during login)
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            // Fallback to Claims if Session not available
            if (userId == null)
            {
                var userClaim = HttpContext.User
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                    
                if (userClaim != null && int.TryParse(userClaim.Value, out int parsedId))
                {
                    userId = parsedId;
                }
            }

            if (userId == null)
                return View(new Cart { CartItems = new List<CartItem>() });

            var cart = _context.Carts
     .Include(c => c.CartItems)
         .ThenInclude(ci => ci.Food)     // ✅ ĐÚNG
     .Include(c => c.CartItems)
         .ThenInclude(ci => ci.Combo)    // ✅ ĐÚNG
     .FirstOrDefault(c => c.UserId == userId);


            return View(cart ?? new Cart { CartItems = new List<CartItem>() });
        }



    }
}
