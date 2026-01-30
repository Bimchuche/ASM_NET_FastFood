using Microsoft.AspNetCore.Mvc;
using ASM1_NET.Data;

namespace ASM1_NET.Controllers
{
    public class ComboController : Controller
    {
        private readonly AppDbContext _context;

        public ComboController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string keyword)
        {
            var combos = _context.Combos
                .Where(c => c.IsActive && !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                combos = combos.Where(c => c.Name.Contains(keyword));
            }

            ViewBag.Keyword = keyword;
            return View(combos.ToList());
        }

        public IActionResult DetailPopup(int id)
        {
            var combo = _context.Combos.FirstOrDefault(c => c.Id == id && c.IsActive && !c.IsDeleted);
            if (combo == null) return NotFound();

            return PartialView("_ComboDetailPopup", combo);
        }
    }
}
