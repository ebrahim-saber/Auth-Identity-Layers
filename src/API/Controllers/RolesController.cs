using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.API.Controllers.Base;
using IdentityAuthLayer.API.Routes;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Application.Roles.Commands.CreateRole;
using IdentityAuthLayer.Application.Roles.Commands.AssignPermissionToRole;
using IdentityAuthLayer.Infrastructure.Persistence;

namespace IdentityAuthLayer.API.Controllers
{
    public record UpdateRoleRequest(string Name, string? Description);

    /// <summary>
    /// إدارة الرولز والصلاحيات المرتبطة بها - كل رول مربوط بسيستم واحد.
    /// </summary>
    public class RolesController : ApplicationBaseController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>عرض جميع الرولز المسجلة (يمكن الفلترة بحسب systemId).</summary>
        [HttpGet(RolesRoutes.Create)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? systemId, [FromServices] ApplicationDbContext db)
        {
            var query = db.Roles.AsQueryable();
            if (systemId.HasValue && systemId.Value != Guid.Empty)
                query = query.Where(r => r.SystemId == systemId.Value);

            var list = await query
                .Include(r => r.System)
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Select(r => new
                {
                    r.Id,
                    r.SystemId,
                    SystemCode = r.System.Code,
                    SystemName = r.System.Name,
                    r.Name,
                    r.Description,
                    r.CreatedAt,
                    Permissions = r.RolePermissions.Select(rp => new
                    {
                        rp.Permission.Id,
                        rp.Permission.Code,
                        rp.Permission.Name
                    })
                })
                .ToListAsync();

            return BaseResponse(list);
        }

        /// <summary>إنشاء رول جديد مربوط بسيستم معين.</summary>
        [HttpPost(RolesRoutes.Create)]
        public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
            => BaseResponse(await _mediator.Send(command));

        /// <summary>تعديل اسم أو وصف رول محدد.</summary>
        [HttpPut(RolesRoutes.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid roleId, [FromBody] UpdateRoleRequest request, CancellationToken ct)
            => BaseResponse(await _roleService.UpdateRoleAsync(roleId, request.Name, request.Description, ct));

        /// <summary>حذف رول نهائياً مع كافة ارتباطاته بالصلاحيات والمستخدمين.</summary>
        [HttpDelete(RolesRoutes.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid roleId, CancellationToken ct)
            => BaseResponse(await _roleService.DeleteRoleAsync(roleId, ct));

        /// <summary>ربط صلاحية برول (يُشترط أن تكون الصلاحية والرول على نفس السيستم).</summary>
        [HttpPost(RolesRoutes.AssignPermission)]
        public async Task<IActionResult> AssignPermission([FromRoute] Guid roleId, [FromBody] Guid permissionId)
            => BaseResponse(await _mediator.Send(new AssignPermissionToRoleCommand(roleId, permissionId)));

        /// <summary>إلغاء ربط صلاحية من رول.</summary>
        [HttpDelete(RolesRoutes.RemovePermission)]
        public async Task<IActionResult> RemovePermission([FromRoute] Guid roleId, [FromRoute] Guid permissionId, CancellationToken ct)
            => BaseResponse(await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId, ct));
    }
}
