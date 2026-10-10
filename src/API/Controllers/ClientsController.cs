using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.API.Controllers.Base;
using IdentityAuthLayer.API.Routes;
using IdentityAuthLayer.Application.Clients.Commands.CreateClient;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Infrastructure.Persistence;

namespace IdentityAuthLayer.API.Controllers
{
    public record UpdateClientRequest(string Name, string? ContactEmail, string? ContactPhone);
    public record LinkClientSystemRequest(Guid SystemId, DateTime? ContractStartDate, DateTime? ContractEndDate, int? MaxUsersQuota);

    /// <summary>
    /// إدارة العملاء والتعاقدات مع الأنظمة.
    /// </summary>
    public class ClientsController : ApplicationBaseController
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        /// <summary>عرض جميع العملاء والأنظمة المتعاقد عليها.</summary>
        [HttpGet(ClientsRoutes.Create)]
        public async Task<IActionResult> GetAll([FromServices] ApplicationDbContext db)
        {
            var list = await db.Clients
                .Include(c => c.ClientSystems)
                .ThenInclude(cs => cs.System)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.ContactEmail,
                    c.ContactPhone,
                    c.IsActive,
                    c.CreatedAt,
                    ContractedSystems = c.ClientSystems.Select(cs => new
                    {
                        cs.SystemId,
                        cs.System.Code,
                        cs.System.Name,
                        cs.ContractedAt,
                        cs.ContractStartDate,
                        cs.ContractEndDate,
                        cs.MaxUsersQuota,
                        cs.IsActive
                    })
                })
                .ToListAsync();

            return BaseResponse(list);
        }

        /// <summary>بيانات عميل محدد مع تفاصيل عقود الأنظمة والكوتا.</summary>
        [HttpGet(ClientsRoutes.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid clientId, CancellationToken ct)
            => BaseResponse(await _clientService.GetClientByIdAsync(clientId, ct));

        /// <summary>إنشاء عميل جديد (الـ Id يتولد تلقائياً ويرجع في النتيجة).</summary>
        [HttpPost(ClientsRoutes.Create)]
        public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
            => BaseResponse(await _mediator.Send(command));

        /// <summary>تعديل بيانات العميل الأساسية.</summary>
        [HttpPut(ClientsRoutes.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid clientId, [FromBody] UpdateClientRequest request, CancellationToken ct)
            => BaseResponse(await _clientService.UpdateClientAsync(clientId, request.Name, request.ContactEmail, request.ContactPhone, ct));

        /// <summary>تفعيل أو تجميد حساب العميل بالكامل (تجميد العميل يمنع دخول جميع مستخدميه).</summary>
        [HttpPost(ClientsRoutes.ToggleActive)]
        public async Task<IActionResult> ToggleActive([FromRoute] Guid clientId, CancellationToken ct)
            => BaseResponse(await _clientService.ToggleClientActiveAsync(clientId, ct));

        /// <summary>ربط عميل بنظام مع تحديد تواريخ بداية ونهاية العقد وكوتا المستخدمين.</summary>
        [HttpPost(ClientsRoutes.LinkSystem)]
        public async Task<IActionResult> LinkSystem([FromRoute] Guid clientId, [FromBody] LinkClientSystemRequest request, CancellationToken ct)
            => BaseResponse(await _clientService.LinkClientToSystemAsync(clientId, request.SystemId, request.ContractStartDate, request.ContractEndDate, request.MaxUsersQuota, ct));

        /// <summary>إلغاء ربط/تعاقد العميل مع نظام محدد نهائياً.</summary>
        [HttpDelete(ClientsRoutes.UnlinkSystem)]
        public async Task<IActionResult> UnlinkSystem([FromRoute] Guid clientId, [FromRoute] Guid systemId, CancellationToken ct)
            => BaseResponse(await _clientService.UnlinkClientFromSystemAsync(clientId, systemId, ct));

        /// <summary>تفعيل أو تجميد تعاقد العميل مع نظام محدد مؤقتاً.</summary>
        [HttpPost(ClientsRoutes.ToggleSystemActive)]
        public async Task<IActionResult> ToggleSystemActive([FromRoute] Guid clientId, [FromRoute] Guid systemId, CancellationToken ct)
            => BaseResponse(await _clientService.ToggleClientSystemActiveAsync(clientId, systemId, ct));
    }
}
