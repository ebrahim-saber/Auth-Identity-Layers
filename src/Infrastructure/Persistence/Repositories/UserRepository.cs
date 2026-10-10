using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) => _context = context;

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct = default) =>
            _context.Users
                .Include(u => u.Client)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.System)
                .FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
            _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var cleanEmail = email.Trim().ToLower();
            return _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail, ct);
        }

        public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
            return _context.Users.FirstOrDefaultAsync(u => u.RefreshTokenInfo!.RefreshToken == hash, ct);
        }

        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
            _context.Users.AnyAsync(u => u.Username == username, ct);

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            var cleanEmail = email.Trim().ToLower();
            return _context.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default) =>
            await _context.Users.AddAsync(user, ct);

        public void Remove(User user) =>
            _context.Users.Remove(user);
    }
}
