using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Common.Interfaces.Services
{
    public record AuthTokensDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, Guid UserId, Guid ClientId, string Username, string UserType);

    public record EmailCheckResultDto(bool Exists, bool IsEmailVerified, bool HasPassword);

    /// <summary>
    /// خدمة الأثنتيكيشن المركزية (API الأولانية بتاعتنا: User Management &amp; Security).
    /// أي سيستم فرعي (تراكينج، حج، معاقين) بيستقبل نفس التوكن الناتج من هنا ويتحقق منه، بدل ما يعمل Login خاص بيه.
    /// </summary>
    public interface IAuthService
    {
        // ---------- Registration & Verification ----------
        Task<Result> RegisterAsync(Guid clientId, string username, string firstName, string lastName, string email, string phone, CancellationToken ct = default);
        Task<Result> VerifyAccountAsync(string email, string verificationCode, CancellationToken ct = default);
        Task<Result> ResendVerificationCodeAsync(string email, CancellationToken ct = default);

        // ---------- Password Management ----------
        Task<Result> CreatePasswordAsync(string email, string password, CancellationToken ct = default);
        Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default);
        Task<Result> VerifyResetCodeAsync(string email, string code, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default);

        // ---------- Authentication ----------
        Task<Result<AuthTokensDto>> LoginAsync(string username, string password, CancellationToken ct = default);
        Task<Result<AuthTokensDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<Result> LogoutAsync(Guid userId, CancellationToken ct = default);

        // ---------- Utilities ----------
        Task<Result<EmailCheckResultDto>> CheckEmailAsync(string email, CancellationToken ct = default);
    }
}
