using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(Guid systemId, string code, CancellationToken ct = default);
        Task AddAsync(Permission permission, CancellationToken ct = default);
        void Remove(Permission permission);
    }
}
