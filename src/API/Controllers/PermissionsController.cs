using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.API.Controllers.Base;
using IdentityAuthLayer.API.Routes;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Application.Permissions.Commands.CreatePermission;
using IdentityAuthLayer.Infrastructure.Persistence;

namespace IdentityAuthLayer.API.Controllers
{
    public record UpdatePermissionRequest(string Name, string? Description);

    /// <summary>
    /// إدارة الصلاحيات - كل صلاحية تتبع نظاماً فرعياً محدداً.
    /// </summary>
    public class PermissionsController : ApplicationBaseController
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>عرض جميع الصلاحيات المسجلة (يمكن الفلترة بحسب systemId).</summary>
        [HttpGet(PermissionsRoutes.Create)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? systemId, [FromServices] ApplicationDbContext db)
        {
            var query = db.Permissions.AsQueryable();
            if (systemId.HasValue && systemId.Value != Guid.Empty)
                query = query.Where(p => p.SystemId == systemId.Value);

            var list = await query
                .Include(p => p.System)
                .Select(p => new
                {
                    p.Id,
                    p.SystemId,
                    SystemCode = p.System.Code,
                    SystemName = p.System.Name,
                    p.Code,
                    p.Name,
                    p.Description,
                    p.CreatedAt
                })
                .ToListAsync();

            return BaseResponse(list);
        }

        /// <summary>إنشاء صلاحية جديدة مربوطة بسيستم معين.</summary>
        [HttpPost(PermissionsRoutes.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePermissionCommand command)
            => BaseResponse(await _mediator.Send(command));

        /// <summary>تعديل اسم أو وصف صلاحية محددة.</summary>
        [HttpPut(PermissionsRoutes.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid permissionId, [FromBody] UpdatePermissionRequest request, CancellationToken ct)
            => BaseResponse(await _permissionService.UpdatePermissionAsync(permissionId, request.Name, request.Description, ct));

        /// <summary>حذف صلاحية نهائياً مع إزالتها من كافة الرولز المرتبطة بها.</summary>
        [HttpDelete(PermissionsRoutes.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid permissionId, CancellationToken ct)
            => BaseResponse(await _permissionService.DeletePermissionAsync(permissionId, ct));
    }
}
