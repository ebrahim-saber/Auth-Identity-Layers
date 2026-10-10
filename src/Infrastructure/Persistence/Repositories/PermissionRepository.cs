using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Domain.Entities;

namespace IdentityAuthLayer.Infrastructure.Persistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context) => _context = context;

        public Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, ct);

        public Task<bool> ExistsByCodeAsync(Guid systemId, string code, CancellationToken ct = default) =>
            _context.Permissions.AnyAsync(p => p.SystemId == systemId && p.Code == code, ct);

        public async Task AddAsync(Permission permission, CancellationToken ct = default) =>
            await _context.Permissions.AddAsync(permission, ct);

        public void Remove(Permission permission) =>
            _context.Permissions.Remove(permission);
    }
}
