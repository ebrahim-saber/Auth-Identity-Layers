using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context) => _context = context;

        public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);

        public Task<bool> ExistsByNameAsync(Guid systemId, string name, CancellationToken ct = default) =>
            _context.Roles.AnyAsync(r => r.SystemId == systemId && r.Name == name, ct);

        public async Task AddAsync(Role role, CancellationToken ct = default) =>
            await _context.Roles.AddAsync(role, ct);

        public void Remove(Role role) =>
            _context.Roles.Remove(role);
    }
}
