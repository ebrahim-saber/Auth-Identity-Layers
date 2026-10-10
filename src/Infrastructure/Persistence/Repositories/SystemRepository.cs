using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class SystemRepository : ISystemRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemRepository(ApplicationDbContext context) => _context = context;

        public Task<SystemEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Systems.FirstOrDefaultAsync(s => s.Id == id, ct);

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
            _context.Systems.AnyAsync(s => s.Id == id, ct);

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default) =>
            _context.Systems.AnyAsync(s => s.Code == code, ct);

        public async Task AddAsync(SystemEntity system, CancellationToken ct = default) =>
            await _context.Systems.AddAsync(system, ct);

        public void Remove(SystemEntity system) =>
            _context.Systems.Remove(system);
    }
}
