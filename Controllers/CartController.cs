using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ASM1_NET.Data;
using ASM1_NET.Models;

namespace ASM1_NET.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim == null ? null : int.Parse(claim.Value);
        }

        public IActionResult Index()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Food)
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Combo)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { CartItems = new List<CartItem>() };
            }

            if (cart.CartItems == null)
            {
                cart.CartItems = new List<CartItem>();
            }

            return View(cart);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add(int foodId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Json(new { success = false });

            int userId = int.Parse(userIdClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            var food = _context.Foods.FirstOrDefault(f => f.Id == foodId && !f.IsDeleted);
            if (food == null)
                return Json(new { success = false });

            var item = cart.CartItems.FirstOrDefault(i => i.FoodId == foodId);

            if (item == null)
            {
                cart.CartItems.Add(new CartItem
                {
                    FoodId = foodId,
                    Quantity = 1,
                    Price = food.Price
                });
            }
            else
            {
                item.Quantity++;
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddCombo(int comboId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return Json(new { success = false });

            int userId = int.Parse(userClaim.Value);

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var combo = _context.Combos.FirstOrDefault(c => c.Id == comboId && c.IsActive && !c.IsDeleted);

            if (combo == null)
                return Json(new { success = false });

            var item = _context.CartItems.FirstOrDefault(i => i.CartId == cart.Id && i.ComboId == comboId);

            if (item == null)
            {
                item = new CartItem
                {
                    CartId = cart.Id,
                    ComboId = comboId,
                    Quantity = 1,
                    Price = combo.Price
                };
                _context.CartItems.Add(item);
            }
            else
            {
                item.Quantity++;
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult IncreaseCombo(int comboId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Json(new { success = false });

            int userId = int.Parse(userClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(i => i.ComboId == comboId);
            if (item == null) return Json(new { success = false });

            item.Quantity++;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DecreaseCombo(int comboId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Json(new { success = false });

            int userId = int.Parse(userClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(i => i.ComboId == comboId);
            if (item == null) return Json(new { success = false });

            item.Quantity--;

            if (item.Quantity <= 0)
                _context.CartItems.Remove(item);

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RemoveCombo(int comboId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Json(new { success = false });

            int userId = int.Parse(userClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(i => i.ComboId == comboId);
            if (item == null) return Json(new { success = false });

            _context.CartItems.Remove(item);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Increase(int foodId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Json(new { success = false });

            int userId = int.Parse(userIdClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(i => i.FoodId == foodId);
            if (item == null) return Json(new { success = false });

            item.Quantity++;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Decrease(int foodId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Json(new { success = false });

            int userId = int.Parse(userIdClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(i => i.FoodId == foodId);
            if (item == null) return Json(new { success = false });

            item.Quantity--;

            if (item.Quantity <= 0)
                _context.CartItems.Remove(item);

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Remove(int foodId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Json(new { success = false });

            int userId = int.Parse(userIdClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(i => i.FoodId == foodId);
            if (item == null) return Json(new { success = false });

            _context.CartItems.Remove(item);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult MiniCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return PartialView("MiniCart", new Cart { CartItems = new List<CartItem>() });
            }

            int userId = int.Parse(userIdClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { CartItems = new List<CartItem>() };
            }
            else if (cart.CartItems == null)
            {
                cart.CartItems = new List<CartItem>();
            }

            return PartialView("MiniCart", cart);
        }

        [HttpPost]
        public IActionResult Checkout(string address, string phone)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Food)
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Combo)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index");

            var order = new Order
            {
                OrderCode = "OD" + DateTime.Now.Ticks,
                OrderDate = DateTime.Now,
                CustomerId = userId,
                Address = address,
                Phone = phone,
                Status = "Pending",
                TotalAmount = cart.CartItems.Sum(i => i.Price * i.Quantity)
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    FoodId = item.FoodId,
                    ComboId = item.ComboId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);
            _context.SaveChanges();

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
