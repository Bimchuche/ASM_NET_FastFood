using Microsoft.AspNetCore.Mvc;

namespace ASM1_NET.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Giới thiệu";
            return View();
        }
    }
}
