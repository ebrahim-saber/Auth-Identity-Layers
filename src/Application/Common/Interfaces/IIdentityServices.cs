using System;
using System.Collections.Generic;

namespace IdentityAuthLayer.Application.Common.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string plainPassword);
        bool Verify(string plainPassword, string hash);
    }

    /// <summary>
    /// بيولد الـ JWT اللي هيتبعت للـ (API الأولانية) User Management & Security
    /// وهو اللي بيتحقق منه أي سيستم تاني (Tracking, Hajj, Disability...) عن طريق مصادقة الـ Token.
    /// </summary>
    public interface IJwtTokenService
    {
        string GenerateToken(Guid userId, Guid clientId, string username, string userType, IEnumerable<string> extraClaims = null!);

        /// <summary>توكن عشوائي طويل بيتخزن على اليوزر، بيُستخدم لتجديد الـ Access Token من غير ما يعمل Login تاني.</summary>
        string GenerateRefreshToken();
    }
}
