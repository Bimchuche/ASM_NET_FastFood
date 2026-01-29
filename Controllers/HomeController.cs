using ASM1_NET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Foods = _context.Foods
                .Where(f => f.IsAvailable && !f.IsDeleted)  // ✅ Ẩn món đã xóa
                .OrderByDescending(f => f.Id)
                .Take(6)
                .ToList();

            ViewBag.Combos = _context.Combos
                .Where(c => c.IsActive && !c.IsDeleted)  // ✅ Ẩn combo đã xóa
                .OrderByDescending(c => c.Id)
                .Take(4)
                .ToList();

            return View();
        }
    }
}
