using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Entities;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _uow;

        public RoleService(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<Guid>> CreateRoleAsync(Guid systemId, string name, string? description, CancellationToken ct = default)
        {
            if (!await _uow.Systems.ExistsAsync(systemId, ct))
                return Result.Failure<Guid>("السيستم غير موجود.");

            if (await _uow.Roles.ExistsByNameAsync(systemId, name, ct))
                return Result.Failure<Guid>("يوجد رول بنفس الاسم على هذا السيستم بالفعل.");

            var role = new Role { SystemId = systemId, Name = name, Description = description };

            await _uow.Roles.AddAsync(role, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(role.Id, "تم إنشاء الرول بنجاح");
        }

        public async Task<Result<Guid>> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.GetByIdAsync(roleId, ct);
            if (role is null)
                return Result.Failure<Guid>("الرول غير موجود.");

            var permission = await _uow.Permissions.GetByIdAsync(permissionId, ct);
            if (permission is null)
                return Result.Failure<Guid>("الصلاحية غير موجودة.");

            // ====== القاعدة الجوهرية في التصميم كله ======
            if (role.SystemId != permission.SystemId)
                return Result.Failure<Guid>("لا يمكن ربط صلاحية من سيستم مختلف عن سيستم الرول.");

            if (await _uow.RolePermissions.ExistsAsync(roleId, permissionId, ct))
                return Result.Failure<Guid>("الصلاحية مضافة بالفعل لهذا الرول.");

            var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId };

            await _uow.RolePermissions.AddAsync(rolePermission, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(rolePermission.Id, "تم ربط الصلاحية بالرول بنجاح");
        }

        public async Task<Result> UpdateRoleAsync(Guid roleId, string name, string? description, CancellationToken ct = default)
        {
            var role = await _uow.Roles.GetByIdAsync(roleId, ct);
            if (role is null)
                return Result.Failure("الرول غير موجود.");

            role.Name = name;
            role.Description = description;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تحديث بيانات الرول بنجاح");
        }

        public async Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.GetByIdAsync(roleId, ct);
            if (role is null)
                return Result.Failure("الرول غير موجود.");

            _uow.Roles.Remove(role);
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم حذف الرول بنجاح");
        }

        public async Task<Result> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
        {
            var rolePermission = await _uow.RolePermissions.GetByRoleAndPermissionAsync(roleId, permissionId, ct);
            if (rolePermission is null)
                return Result.Failure("هذه الصلاحية غير مرتبطة بهذا الرول.");

            _uow.RolePermissions.Remove(rolePermission);
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم سحب الصلاحية من الرول بنجاح");
        }
    }
}
