using ASM1_NET.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASM1_NET.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string name, string email, string phone, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin bắt buộc!";
                return RedirectToAction("Index");
            }

            try
            {
                // Send email to admin
                await _emailService.SendContactEmailAsync(name, email, phone ?? "", message);
                TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi qua email sớm nhất.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Contact email error: {ex.Message}");
                // Still show success to user but log the error
                TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
            }

            return RedirectToAction("Index");
        }
    }
}
