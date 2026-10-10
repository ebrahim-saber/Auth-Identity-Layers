using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionRepository(ApplicationDbContext context) => _context = context;

        public Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken ct = default) =>
            _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, ct);

        public Task<RolePermission?> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default) =>
            _context.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, ct);

        public async Task AddAsync(RolePermission rolePermission, CancellationToken ct = default) =>
            await _context.RolePermissions.AddAsync(rolePermission, ct);

        public void Remove(RolePermission rolePermission) =>
            _context.RolePermissions.Remove(rolePermission);
    }
}
