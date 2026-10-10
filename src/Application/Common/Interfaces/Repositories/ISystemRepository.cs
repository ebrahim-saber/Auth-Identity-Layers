using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    public interface ISystemRepository
    {
        Task<SystemEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
        Task AddAsync(SystemEntity system, CancellationToken ct = default);
        void Remove(SystemEntity system);
    }
}
