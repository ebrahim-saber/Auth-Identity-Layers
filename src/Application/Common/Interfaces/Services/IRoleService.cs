using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Common.Interfaces.Services
{
    public interface IRoleService
    {
        Task<Result<Guid>> CreateRoleAsync(Guid systemId, string name, string? description, CancellationToken ct = default);
        Task<Result> UpdateRoleAsync(Guid roleId, string name, string? description, CancellationToken ct = default);
        Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken ct = default);

        /// <summary>القاعدة هنا: الرول والبرميشن لازم يكونوا على نفس السيستم.</summary>
        Task<Result<Guid>> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
        Task<Result> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
    }
}
