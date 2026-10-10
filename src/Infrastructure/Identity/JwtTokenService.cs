using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IdentityAuthLayer.Application.Common.Interfaces;

namespace IdentityAuthLayer.Infrastructure.Identity
{
    public class JwtSettings
    {
        public string Secret { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int ExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }

    /// <summary>
    /// بيولد الـ JWT اللي هو "مصدر الثقة" المشترك بين API الأولانية وكل الأنظمة الفرعية.
    /// كل سيستم فرعي بيتحقق من نفس التوكن بنفس الـ Secret/Issuer/Audience (Bearer Auth عادي)
    /// وبعدين يستخدم UserId اللي جوه التوكن عشان يسأل CheckUserPermissionQuery على الـ API الأولانية.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateToken(Guid userId, Guid clientId, string username, string userType, IEnumerable<string> extraClaims = null!)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new("clientId", clientId.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, username),
                new("userType", userType),
                new(ClaimTypes.Role, userType),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (extraClaims is not null)
            {
                foreach (var c in extraClaims)
                    claims.Add(new Claim("extra", c));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
