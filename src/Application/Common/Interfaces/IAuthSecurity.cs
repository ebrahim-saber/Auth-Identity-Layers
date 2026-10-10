using System;

namespace IdentityAuthLayer.Application.Common.Interfaces
{
    public interface IAuthSecurity
    {
        string HashVerificationCode(Guid userId, string code, string purpose);
        bool VerifyVerificationCode(Guid userId, string code, string purpose, string storedHash);
        string HashRefreshToken(string refreshToken);
    }
}
