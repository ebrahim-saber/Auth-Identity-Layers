using System;
using System.Security.Cryptography;
using IdentityAuthLayer.Application.Common.Interfaces;

namespace IdentityAuthLayer.Infrastructure.Identity
{
    /// <summary>هاش الباسورد باستخدام PBKDF2 (Rfc2898DeriveBytes) - مفيش داعي لباكدج خارجي.</summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public string Hash(string plainPassword)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool Verify(string plainPassword, string hash)
        {
            try
            {
                var parts = hash.Split('.', 2);
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var expectedKey = Convert.FromBase64String(parts[1]);

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

                return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
