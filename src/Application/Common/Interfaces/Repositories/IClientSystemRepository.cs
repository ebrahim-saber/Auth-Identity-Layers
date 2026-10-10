using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    public interface IClientSystemRepository
    {
        Task<bool> ExistsAsync(Guid clientId, Guid systemId, CancellationToken ct = default);

        Task<bool> IsClientContractedOnSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default);

        Task<ClientSystem?> GetByClientAndSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default);

        Task AddAsync(ClientSystem clientSystem, CancellationToken ct = default);

        void Remove(ClientSystem clientSystem);
    }
}
