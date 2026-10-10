using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityAuthLayer.Application.Common.Interfaces;

namespace IdentityAuthLayer.Infrastructure.Email
{
    /// <summary>
    /// تنفيذ بسيط بـ System.Net.Mail (مفيش داعي لباكدج خارجي زي MailKit).
    /// لو فشل الإرسال، بنسجله في اللوج ومنوقفش العملية الأساسية (نفس فكرة CarCare).
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly MailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<MailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public Task SendVerificationCodeAsync(string toEmail, string code, string fullName) =>
            SendAsync(toEmail, "تفعيل الحساب", $"مرحبًا {fullName}،<br/>كود تفعيل حسابك هو: <b>{code}</b><br/>الكود صالح لمدة 15 دقيقة.");

        public Task SendResetPasswordCodeAsync(string toEmail, string code, string fullName) =>
            SendAsync(toEmail, "إعادة تعيين كلمة المرور", $"مرحبًا {fullName}،<br/>كود إعادة تعيين كلمة المرور هو: <b>{code}</b><br/>الكود صالح لمدة 15 دقيقة.");

        public Task SendPasswordChangedNotificationAsync(string toEmail, string fullName) =>
            SendAsync(toEmail, "تم تغيير كلمة المرور", $"مرحبًا {fullName}،<br/>تم تغيير كلمة مرور حسابك بنجاح. لو مكنتش أنت، تواصل معنا فورًا.");

        private async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.Username) || _settings.Username.Contains("REPLACE_WITH"))
                {
                    _logger.LogWarning("إعدادات SMTP غير مهيأة بعد. الرسالة الموجهة إلى {Email}: {Subject} -> {Body}", toEmail, subject, htmlBody);
                    return;
                }

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.UseSsl,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // منوقفش الفلو الأساسي بسبب فشل إرسال إيميل - بس بنسجله
                _logger.LogWarning(ex, "تعذر إرسال الإيميل إلى {Email}. محتوى الرسالة: {Subject}", toEmail, subject);
            }
        }
    }
}
