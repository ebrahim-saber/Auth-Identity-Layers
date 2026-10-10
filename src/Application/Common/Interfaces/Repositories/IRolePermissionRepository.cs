using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    public interface IRolePermissionRepository
    {
        Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
        Task<RolePermission?> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
        Task AddAsync(RolePermission rolePermission, CancellationToken ct = default);
        void Remove(RolePermission rolePermission);
    }
}
