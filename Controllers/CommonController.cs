using Microsoft.AspNetCore.Mvc;

namespace ASM1_NET.Controllers
{
    public class CommonController : Controller
    {
        [HttpGet]
        public IActionResult MiniCart()
        {
            return ViewComponent("MiniCart");
        }
    }
}
