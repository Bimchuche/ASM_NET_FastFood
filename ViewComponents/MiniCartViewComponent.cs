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
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null)
            {
                var userClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userClaim != null && int.TryParse(userClaim.Value, out int parsedId))
                {
                    userId = parsedId;
                }
            }

            if (userId == null)
                return View(new Cart { CartItems = new List<CartItem>() });

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Combo)
                .FirstOrDefault(c => c.UserId == userId);

            return View(cart ?? new Cart { CartItems = new List<CartItem>() });
        }
    }
}
