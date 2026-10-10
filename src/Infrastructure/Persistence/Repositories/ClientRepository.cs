using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context) => _context = context;

        public Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<Client?> GetWithSystemsAsync(Guid id, CancellationToken ct = default) =>
            _context.Clients
                .Include(c => c.ClientSystems).ThenInclude(cs => cs.System)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
            _context.Clients.AnyAsync(c => c.Id == id, ct);

        public async Task AddAsync(Client client, CancellationToken ct = default) =>
            await _context.Clients.AddAsync(client, ct);

        public void Remove(Client client) =>
            _context.Clients.Remove(client);
    }
}
