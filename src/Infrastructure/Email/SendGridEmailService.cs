using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using IdentityAuthLayer.Application.Common.Interfaces;

namespace IdentityAuthLayer.Infrastructure.Email
{
    /// <summary>
    /// خدمة إرسال الإيميلات باستخدام SendGrid API.
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly SendGridSettings _settings;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IOptions<SendGridSettings> settings, ILogger<SendGridEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public Task SendVerificationCodeAsync(string toEmail, string code, string fullName)
        {
            _logger.LogInformation("--> تم توليد كود تفعيل لـ {Email} برمز: [{Code}]", toEmail, code);
            var subject = $"[{code}] رمز تفعيل حسابك في النظام";
            var html = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px; background-color: #ffffff;'>
                    <h2 style='color: #2b6cb0; margin-bottom: 16px;'>مرحبًا {fullName}</h2>
                    <p style='color: #4a5568; font-size: 16px; line-height: 1.6;'>شكرًا لتسجيلك في النظام. لتفعيل حسابك، يرجى استخدام رمز التحقق التالي:</p>
                    <div style='margin: 24px 0; text-align: center;'>
                        <span style='display: inline-block; background-color: #edf2f7; color: #2d3748; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 12px 24px; border-radius: 6px; border: 1px dashed #cbd5e0;'>{code}</span>
                    </div>
                    <p style='color: #718096; font-size: 14px;'>هذا الرمز صالح لمدة 15 دقيقة فقط. إذا لم تكن قد طلبت هذا الحساب، يمكنك تجاهل هذه الرسالة بأمان.</p>
                </div>";

            return SendAsync(toEmail, subject, html);
        }

        public Task SendResetPasswordCodeAsync(string toEmail, string code, string fullName)
        {
            _logger.LogInformation("--> تم توليد كود إعادة تعيين كلمة المرور لـ {Email} برمز: [{Code}]", toEmail, code);
            var subject = $"[{code}] رمز إعادة تعيين كلمة المرور";
            var html = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px; background-color: #ffffff;'>
                    <h2 style='color: #c53030; margin-bottom: 16px;'>مرحبًا {fullName}</h2>
                    <p style='color: #4a5568; font-size: 16px; line-height: 1.6;'>تلقينا طلبًا لإعادة تعيين كلمة مرور حسابك. رمز التحقق الخاص بك هو:</p>
                    <div style='margin: 24px 0; text-align: center;'>
                        <span style='display: inline-block; background-color: #fff5f5; color: #c53030; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 12px 24px; border-radius: 6px; border: 1px dashed #feb2b2;'>{code}</span>
                    </div>
                    <p style='color: #718096; font-size: 14px;'>هذا الرمز صالح لمدة 15 دقيقة فقط. إذا لم تكن قد طلبت إعادة التعيين، يُرجى تجاهل هذه الرسالة.</p>
                </div>";

            return SendAsync(toEmail, subject, html);
        }

        public Task SendPasswordChangedNotificationAsync(string toEmail, string fullName)
        {
            var subject = "تم تغيير كلمة المرور بنجاح";
            var html = $@"
                <div style='font-family: Arial, sans-serif; direction: rtl; text-align: right; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px; background-color: #ffffff;'>
                    <h2 style='color: #38a169; margin-bottom: 16px;'>مرحبًا {fullName}</h2>
                    <p style='color: #4a5568; font-size: 16px; line-height: 1.6;'>نحيطك علمًا بأنه تم تغيير كلمة المرور الخاصة بحسابك بنجاح.</p>
                    <p style='color: #e53e3e; font-size: 14px;'>إذا لم تكن أنت من قام بهذا التغيير، يُرجى التواصل مع إدارة النظام فورًا.</p>
                </div>";

            return SendAsync(toEmail, subject, html);
        }

        private async Task SendAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var apiKey = _settings.EffectiveApiKey;
                if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("REPLACE_WITH"))
                {
                    _logger.LogWarning("SendGrid API Key غير مهيأ بعد في appsettings.json. تم تجهيز الإيميل إلى {Email} بعنوان '{Subject}'.", toEmail, subject);
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_settings.SenderEmail, _settings.SenderName);
                var to = new EmailAddress(toEmail);
                var plainText = Regex.Replace(htmlContent, "<.*?>", string.Empty);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);

                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("تم إرسال الإيميل بنجاح عبر SendGrid إلى {Email} - الحالة: {StatusCode}", toEmail, response.StatusCode);
                }
                else
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("فشل إرسال الإيميل عبر SendGrid إلى {Email} - كود الحالة: {StatusCode} - الرد: {ResponseBody}",
                        toEmail, response.StatusCode, responseBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث استثناء أثناء إرسال الإيميل عبر SendGrid إلى {Email}", toEmail);
            }
        }
    }
}
