using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Entities;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Services
{
    public class SystemService : ISystemService
    {
        private readonly IUnitOfWork _uow;

        public SystemService(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<Guid>> CreateSystemAsync(string code, string name, string? description, CancellationToken ct = default)
        {
            if (await _uow.Systems.ExistsByCodeAsync(code, ct))
                return Result.Failure<Guid>($"يوجد سيستم مسجل بالفعل بنفس الكود: {code}");

            var apiKey = "SYS_KEY_" + Guid.NewGuid().ToString("N");
            var system = new SystemEntity { Code = code, Name = name, Description = description, ApiKey = apiKey };

            await _uow.Systems.AddAsync(system, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(system.Id, "تم إنشاء السيستم وتوليد مفتاح الربط (ApiKey) بنجاح");
        }

        public async Task<Result<SystemDetailsDto>> GetSystemByIdAsync(Guid systemId, CancellationToken ct = default)
        {
            var system = await _uow.Systems.GetByIdAsync(systemId, ct);
            if (system is null)
                return Result.Failure<SystemDetailsDto>("السيستم غير موجود.");

            var dto = new SystemDetailsDto(system.Id, system.Code, system.Name, system.Description, system.ApiKey, system.IsActive, system.CreatedAt);
            return Result.Success(dto);
        }

        public async Task<Result> UpdateSystemAsync(Guid systemId, string name, string? description, CancellationToken ct = default)
        {
            var system = await _uow.Systems.GetByIdAsync(systemId, ct);
            if (system is null)
                return Result.Failure("السيستم غير موجود.");

            system.Name = name;
            system.Description = description;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تحديث بيانات السيستم بنجاح");
        }

        public async Task<Result> ToggleSystemActiveAsync(Guid systemId, CancellationToken ct = default)
        {
            var system = await _uow.Systems.GetByIdAsync(systemId, ct);
            if (system is null)
                return Result.Failure("السيستم غير موجود.");

            system.IsActive = !system.IsActive;
            await _uow.SaveChangesAsync(ct);
            var statusStr = system.IsActive ? "تنشيط" : "تعطيل";
            return Result.Success($"تم {statusStr} السيستم بنجاح");
        }
    }
}
