using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using IdentityAuthLayer.Application.Common.Interfaces;

namespace IdentityAuthLayer.Infrastructure.Identity
{
    public sealed class AuthSecurityOptions
    {
        public string CodeHashSecret { get; set; } = string.Empty;
    }

    public sealed class AuthSecurity : IAuthSecurity
    {
        private readonly AuthSecurityOptions _options;
        public AuthSecurity(IOptions<AuthSecurityOptions> options) => _options = options.Value;

        public string HashVerificationCode(Guid userId, string code, string purpose)
        {
            if (string.IsNullOrWhiteSpace(_options.CodeHashSecret))
                throw new InvalidOperationException("AuthSecurity:CodeHashSecret غير مضبوط.");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.CodeHashSecret));
            var value = $"{purpose}:{userId:N}:{code}";
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        public bool VerifyVerificationCode(Guid userId, string code, string purpose, string storedHash)
        {
            var expected = HashVerificationCode(userId, code, purpose);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(storedHash));
        }

        public string HashRefreshToken(string refreshToken)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        }
    }
}
