using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Entities;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _uow;

        public PermissionService(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<Guid>> CreatePermissionAsync(Guid systemId, string code, string name, string? description, CancellationToken ct = default)
        {
            if (!await _uow.Systems.ExistsAsync(systemId, ct))
                return Result.Failure<Guid>("السيستم غير موجود.");

            if (await _uow.Permissions.ExistsByCodeAsync(systemId, code, ct))
                return Result.Failure<Guid>("يوجد Permission بنفس الكود على هذا السيستم بالفعل.");

            var permission = new Permission { SystemId = systemId, Code = code, Name = name, Description = description };

            await _uow.Permissions.AddAsync(permission, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(permission.Id, "تم إنشاء الصلاحية بنجاح");
        }

        public async Task<Result> UpdatePermissionAsync(Guid permissionId, string name, string? description, CancellationToken ct = default)
        {
            var permission = await _uow.Permissions.GetByIdAsync(permissionId, ct);
            if (permission is null)
                return Result.Failure("الصلاحية غير موجودة.");

            permission.Name = name;
            permission.Description = description;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تحديث بيانات الصلاحية بنجاح");
        }

        public async Task<Result> DeletePermissionAsync(Guid permissionId, CancellationToken ct = default)
        {
            var permission = await _uow.Permissions.GetByIdAsync(permissionId, ct);
            if (permission is null)
                return Result.Failure("الصلاحية غير موجودة.");

            _uow.Permissions.Remove(permission);
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم حذف الصلاحية بنجاح");
        }
    }
}
