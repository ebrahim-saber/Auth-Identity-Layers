using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.API.Controllers.Base;
using IdentityAuthLayer.API.Routes;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Application.Systems.Commands.CreateSystem;
using IdentityAuthLayer.Infrastructure.Persistence;

namespace IdentityAuthLayer.API.Controllers
{
    public record UpdateSystemRequest(string Name, string? Description);

    /// <summary>
    /// إدارة وتسجيل الأنظمة (تراكينج، حج، معاقين...).
    /// </summary>
    public class SystemsController : ApplicationBaseController
    {
        private readonly ISystemService _systemService;

        public SystemsController(ISystemService systemService)
        {
            _systemService = systemService;
        }

        /// <summary>عرض جميع الأنظمة المسجلة في المنظومة.</summary>
        [HttpGet(SystemsRoutes.Create)]
        public async Task<IActionResult> GetAll([FromServices] ApplicationDbContext db)
        {
            var list = await db.Systems
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.Name,
                    s.Description,
                    s.ApiKey,
                    s.IsActive,
                    s.CreatedAt
                })
                .ToListAsync();

            return BaseResponse(list);
        }

        /// <summary>عرض تفاصيل نظام محدد بواسطة الـ systemId.</summary>
        [HttpGet(SystemsRoutes.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid systemId, CancellationToken ct)
            => BaseResponse(await _systemService.GetSystemByIdAsync(systemId, ct));

        /// <summary>إنشاء نظام جديد.</summary>
        [HttpPost(SystemsRoutes.Create)]
        public async Task<IActionResult> Create([FromBody] CreateSystemCommand command)
            => BaseResponse(await _mediator.Send(command));

        /// <summary>تعديل بيانات نظام محدد.</summary>
        [HttpPut(SystemsRoutes.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid systemId, [FromBody] UpdateSystemRequest request, CancellationToken ct)
            => BaseResponse(await _systemService.UpdateSystemAsync(systemId, request.Name, request.Description, ct));

        /// <summary>تفعيل أو تعطيل نظام في المنظومة.</summary>
        [HttpPost(SystemsRoutes.ToggleActive)]
        public async Task<IActionResult> ToggleActive([FromRoute] Guid systemId, CancellationToken ct)
            => BaseResponse(await _systemService.ToggleSystemActiveAsync(systemId, ct));
    }
}
