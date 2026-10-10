using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context) => _context = context;

        public Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default) =>
            _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

        public Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default) =>
            _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

        public async Task AddAsync(UserRole userRole, CancellationToken ct = default) =>
            await _context.UserRoles.AddAsync(userRole, ct);

        public void Remove(UserRole userRole) =>
            _context.UserRoles.Remove(userRole);

        public async Task<List<UserPermissionRecord>> GetUserPermissionsAsync(Guid userId, Guid systemId, CancellationToken ct = default)
        {
            var query =
                from ur in _context.UserRoles
                join role in _context.Roles on ur.RoleId equals role.Id
                join rp in _context.RolePermissions on role.Id equals rp.RoleId
                join perm in _context.Permissions on rp.PermissionId equals perm.Id
                where ur.UserId == userId && role.SystemId == systemId && perm.SystemId == systemId
                select new UserPermissionRecord(role.Name, perm.Code, perm.Name);

            return await query.Distinct().ToListAsync(ct);
        }

        public Task<bool> HasPermissionAsync(Guid userId, Guid systemId, string permissionCode, CancellationToken ct = default)
        {
            var query =
                from ur in _context.UserRoles
                join role in _context.Roles on ur.RoleId equals role.Id
                join rp in _context.RolePermissions on role.Id equals rp.RoleId
                join perm in _context.Permissions on rp.PermissionId equals perm.Id
                where ur.UserId == userId
                      && role.SystemId == systemId
                      && perm.SystemId == systemId
                      && perm.Code == permissionCode
                select perm.Id;

            return query.AnyAsync(ct);
        }

        public Task<int> CountDistinctUsersOnSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default)
        {
            var query =
                from ur in _context.UserRoles
                join u in _context.Users on ur.UserId equals u.Id
                join r in _context.Roles on ur.RoleId equals r.Id
                where u.ClientId == clientId && r.SystemId == systemId
                select ur.UserId;

            return query.Distinct().CountAsync(ct);
        }
    }
}
