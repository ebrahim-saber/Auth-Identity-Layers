using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class ClientSystemRepository : IClientSystemRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientSystemRepository(ApplicationDbContext context) => _context = context;

        public Task<bool> ExistsAsync(Guid clientId, Guid systemId, CancellationToken ct = default) =>
            _context.ClientSystems.AnyAsync(cs => cs.ClientId == clientId && cs.SystemId == systemId, ct);

        public Task<bool> IsClientContractedOnSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default) =>
            _context.ClientSystems.AnyAsync(cs => cs.ClientId == clientId && cs.SystemId == systemId && cs.IsActive
                && (!cs.ContractEndDate.HasValue || cs.ContractEndDate.Value > DateTime.UtcNow), ct);

        public Task<ClientSystem?> GetByClientAndSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default) =>
            _context.ClientSystems.FirstOrDefaultAsync(cs => cs.ClientId == clientId && cs.SystemId == systemId, ct);

        public async Task AddAsync(ClientSystem clientSystem, CancellationToken ct = default) =>
            await _context.ClientSystems.AddAsync(clientSystem, ct);

        public void Remove(ClientSystem clientSystem) =>
            _context.ClientSystems.Remove(clientSystem);
    }
}
