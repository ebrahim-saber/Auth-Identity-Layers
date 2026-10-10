using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Infrastructure.Persistence.Repositories;

namespace IdentityAuthLayer.Infrastructure.Persistence
{
    /// <summary>
    /// بيجمع كل الـ Repositories على نفس الـ DbContext، وبيعمل SaveChanges مرة واحدة
    /// (يعني لو عملية بتلمس أكتر من Repository، كلها بتتحفظ في transaction واحدة).
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Clients = new ClientRepository(_context);
            Systems = new SystemRepository(_context);
            ClientSystems = new ClientSystemRepository(_context);
            Users = new UserRepository(_context);
            Roles = new RoleRepository(_context);
            Permissions = new PermissionRepository(_context);
            RolePermissions = new RolePermissionRepository(_context);
            UserRoles = new UserRoleRepository(_context);
        }

        public IClientRepository Clients { get; }
        public ISystemRepository Systems { get; }
        public IClientSystemRepository ClientSystems { get; }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IPermissionRepository Permissions { get; }
        public IRolePermissionRepository RolePermissions { get; }
        public IUserRoleRepository UserRoles { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
