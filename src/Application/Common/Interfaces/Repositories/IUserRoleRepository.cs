using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    /// <summary>DTO خفيف عشان الاستعلامات، مش Entity - مفيش داعي نرجّع الـ Entity كاملة للـ Service.</summary>
    public record UserPermissionRecord(string RoleName, string PermissionCode, string PermissionName);

    public interface IUserRoleRepository
    {
        Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default);
        Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
        Task AddAsync(UserRole userRole, CancellationToken ct = default);
        void Remove(UserRole userRole);

        Task<List<UserPermissionRecord>> GetUserPermissionsAsync(Guid userId, Guid systemId, CancellationToken ct = default);

        Task<bool> HasPermissionAsync(Guid userId, Guid systemId, string permissionCode, CancellationToken ct = default);

        Task<int> CountDistinctUsersOnSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default);
    }
}
