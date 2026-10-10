using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Entities;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _uow;

        public ClientService(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<Guid>> CreateClientAsync(string name, string? contactEmail, string? contactPhone, CancellationToken ct = default)
        {
            var client = new Client
            {
                Name = name,
                ContactEmail = contactEmail,
                ContactPhone = contactPhone
            };

            await _uow.Clients.AddAsync(client, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(client.Id, "تم إنشاء العميل بنجاح");
        }

        public async Task<Result<ClientDetailsDto>> GetClientByIdAsync(Guid clientId, CancellationToken ct = default)
        {
            var client = await _uow.Clients.GetWithSystemsAsync(clientId, ct);
            if (client is null)
                return Result.Failure<ClientDetailsDto>("العميل غير موجود.");

            var systems = client.ClientSystems.Select(cs => new ClientContractedSystemDto(
                cs.SystemId,
                cs.System.Code,
                cs.System.Name,
                cs.ContractedAt,
                cs.ContractStartDate,
                cs.ContractEndDate,
                cs.MaxUsersQuota,
                cs.IsActive
            )).ToList();

            var dto = new ClientDetailsDto(client.Id, client.Name, client.ContactEmail, client.ContactPhone, client.IsActive, client.CreatedAt, systems);
            return Result.Success(dto);
        }

        public async Task<Result> UpdateClientAsync(Guid clientId, string name, string? contactEmail, string? contactPhone, CancellationToken ct = default)
        {
            var client = await _uow.Clients.GetByIdAsync(clientId, ct);
            if (client is null)
                return Result.Failure("العميل غير موجود.");

            client.Name = name;
            client.ContactEmail = contactEmail;
            client.ContactPhone = contactPhone;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تحديث بيانات العميل بنجاح");
        }

        public async Task<Result> ToggleClientActiveAsync(Guid clientId, CancellationToken ct = default)
        {
            var client = await _uow.Clients.GetByIdAsync(clientId, ct);
            if (client is null)
                return Result.Failure("العميل غير موجود.");

            client.IsActive = !client.IsActive;
            await _uow.SaveChangesAsync(ct);
            var statusStr = client.IsActive ? "تنشيط" : "تعطيل";
            return Result.Success($"تم {statusStr} العميل بنجاح");
        }

        public async Task<Result<Guid>> LinkClientToSystemAsync(Guid clientId, Guid systemId, DateTime? contractStartDate = null, DateTime? contractEndDate = null, int? maxUsersQuota = null, CancellationToken ct = default)
        {
            if (!await _uow.Clients.ExistsAsync(clientId, ct))
                return Result.Failure<Guid>("العميل غير موجود.");

            if (!await _uow.Systems.ExistsAsync(systemId, ct))
                return Result.Failure<Guid>("السيستم غير موجود.");

            if (await _uow.ClientSystems.ExistsAsync(clientId, systemId, ct))
                return Result.Failure<Guid>("العميل مرتبط بالفعل بهذا السيستم.");

            var clientSystem = new ClientSystem
            {
                ClientId = clientId,
                SystemId = systemId,
                ContractStartDate = contractStartDate,
                ContractEndDate = contractEndDate,
                MaxUsersQuota = maxUsersQuota,
                IsActive = true
            };
            await _uow.ClientSystems.AddAsync(clientSystem, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(clientSystem.Id, "تم ربط العميل بالسيستم بنجاح");
        }

        public async Task<Result> UnlinkClientFromSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default)
        {
            var clientSystem = await _uow.ClientSystems.GetByClientAndSystemAsync(clientId, systemId, ct);
            if (clientSystem is null)
                return Result.Failure("العميل غير مرتبط بهذا السيستم.");

            _uow.ClientSystems.Remove(clientSystem);
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم إلغاء ارتباط العميل بالسيستم بنجاح");
        }

        public async Task<Result> ToggleClientSystemActiveAsync(Guid clientId, Guid systemId, CancellationToken ct = default)
        {
            var clientSystem = await _uow.ClientSystems.GetByClientAndSystemAsync(clientId, systemId, ct);
            if (clientSystem is null)
                return Result.Failure("العميل غير مرتبط بهذا السيستم.");

            clientSystem.IsActive = !clientSystem.IsActive;
            await _uow.SaveChangesAsync(ct);
            var statusStr = clientSystem.IsActive ? "تنشيط" : "تعطيل";
            return Result.Success($"تم {statusStr} اشتراك العميل في السيستم بنجاح");
        }
    }
}
