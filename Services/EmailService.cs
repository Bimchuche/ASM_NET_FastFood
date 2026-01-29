using System.Net;
using System.Net.Mail;

namespace ASM1_NET.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendContactEmailAsync(string senderName, string senderEmail, string senderPhone, string message);
        Task SendPasswordResetCodeAsync(string email, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _fromEmail = "fastfoodwebprojectbim@gmail.com";
        private readonly string _appPassword;

        public EmailService(IConfiguration configuration)
        {
            _appPassword = configuration["EmailSettings:SenderPassword"] ?? "";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_fromEmail, _appPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "FastFood Website"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Error: {ex.Message}");
                throw;
            }
        }

        public async Task SendContactEmailAsync(string senderName, string senderEmail, string senderPhone, string message)
        {
            var subject = $"[FastFood Contact] Tin nhắn mới từ {senderName}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #ff6b35, #f7931a); color: white; padding: 20px; border-radius: 10px 10px 0 0;'>
                        <h2 style='margin: 0;'>📧 Tin nhắn mới từ Website</h2>
                    </div>
                    <div style='background: #f8fafc; padding: 20px; border: 1px solid #e2e8f0;'>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 10px 0; border-bottom: 1px solid #e2e8f0; font-weight: bold; width: 120px;'>👤 Họ tên:</td>
                                <td style='padding: 10px 0; border-bottom: 1px solid #e2e8f0;'>{senderName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px 0; border-bottom: 1px solid #e2e8f0; font-weight: bold;'>📧 Email:</td>
                                <td style='padding: 10px 0; border-bottom: 1px solid #e2e8f0;'><a href='mailto:{senderEmail}'>{senderEmail}</a></td>
                            </tr>
                            <tr>
                                <td style='padding: 10px 0; border-bottom: 1px solid #e2e8f0; font-weight: bold;'>📞 SĐT:</td>
                                <td style='padding: 10px 0; border-bottom: 1px solid #e2e8f0;'>{senderPhone ?? "Không cung cấp"}</td>
                            </tr>
                        </table>
                        <div style='margin-top: 20px;'>
                            <h4 style='margin-bottom: 10px; color: #334155;'>💬 Nội dung tin nhắn:</h4>
                            <div style='background: white; padding: 15px; border-radius: 8px; border: 1px solid #e2e8f0;'>
                                {message.Replace("\n", "<br>")}
                            </div>
                        </div>
                    </div>
                    <div style='background: #1e293b; color: #94a3b8; padding: 15px; text-align: center; border-radius: 0 0 10px 10px; font-size: 12px;'>
                        Tin nhắn được gửi từ FastFood Website • {DateTime.Now:dd/MM/yyyy HH:mm}
                    </div>
                </div>
            ";

            await SendEmailAsync(_fromEmail, subject, body);
        }

        public async Task SendPasswordResetCodeAsync(string email, string code)
        {
            var subject = "[FastFood] Mã xác nhận đặt lại mật khẩu";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #ff6b35, #f7931a); color: white; padding: 25px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h2 style='margin: 0;'>🔐 Đặt lại mật khẩu</h2>
                    </div>
                    <div style='background: #f8fafc; padding: 30px; border: 1px solid #e2e8f0; text-align: center;'>
                        <p style='color: #64748b; margin-bottom: 20px;'>Mã xác nhận của bạn là:</p>
                        <div style='font-size: 32px; font-weight: bold; color: #ff6b35; letter-spacing: 8px; padding: 20px; background: white; border-radius: 10px; border: 2px dashed #e2e8f0;'>
                            {code}
                        </div>
                        <p style='color: #94a3b8; font-size: 13px; margin-top: 20px;'>Mã này có hiệu lực trong 15 phút.</p>
                    </div>
                    <div style='background: #1e293b; color: #94a3b8; padding: 15px; text-align: center; border-radius: 0 0 10px 10px; font-size: 12px;'>
                        FastFood Website • Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, body);
        }
    }
}
