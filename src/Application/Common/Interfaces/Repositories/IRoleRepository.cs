using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(Guid systemId, string name, CancellationToken ct = default);
        Task AddAsync(Role role, CancellationToken ct = default);
        void Remove(Role role);
    }
}
