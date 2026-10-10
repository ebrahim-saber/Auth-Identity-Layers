namespace IdentityAuthLayer.Application.Common.Interfaces
{
    /// <summary>إرسال إيميلات (كود تحقق، كود إعادة تعيين، إشعار تغيير الباسورد...).</summary>
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string code, string fullName);
        Task SendResetPasswordCodeAsync(string toEmail, string code, string fullName);
        Task SendPasswordChangedNotificationAsync(string toEmail, string fullName);
    }
}
