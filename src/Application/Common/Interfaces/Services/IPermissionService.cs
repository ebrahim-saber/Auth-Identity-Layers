using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Common.Interfaces.Services
{
    public interface IPermissionService
    {
        Task<Result<Guid>> CreatePermissionAsync(Guid systemId, string code, string name, string? description, CancellationToken ct = default);
        Task<Result> UpdatePermissionAsync(Guid permissionId, string name, string? description, CancellationToken ct = default);
        Task<Result> DeletePermissionAsync(Guid permissionId, CancellationToken ct = default);
    }
}
