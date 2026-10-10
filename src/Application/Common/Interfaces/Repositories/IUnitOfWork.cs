namespace IdentityAuthLayer.Application.Common.Interfaces.Repositories
{
    /// <summary>بيجمع كل الـ Repositories وبيحفظ التغييرات في transaction واحدة.</summary>
    public interface IUnitOfWork
    {
        IClientRepository Clients { get; }
        ISystemRepository Systems { get; }
        IClientSystemRepository ClientSystems { get; }
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        IRolePermissionRepository RolePermissions { get; }
        IUserRoleRepository UserRoles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
