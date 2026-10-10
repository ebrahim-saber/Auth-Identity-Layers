using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Common.Interfaces.Services
{
    public record ClientContractedSystemDto(Guid SystemId, string SystemCode, string SystemName, DateTime ContractedAt, DateTime? ContractStartDate, DateTime? ContractEndDate, int? MaxUsersQuota, bool IsActive);

    public record ClientDetailsDto(Guid Id, string Name, string? ContactEmail, string? ContactPhone, bool IsActive, DateTime CreatedAt, List<ClientContractedSystemDto> ContractedSystems);

    public interface IClientService
    {
        Task<Result<Guid>> CreateClientAsync(string name, string? contactEmail, string? contactPhone, CancellationToken ct = default);
        Task<Result<ClientDetailsDto>> GetClientByIdAsync(Guid clientId, CancellationToken ct = default);
        Task<Result> UpdateClientAsync(Guid clientId, string name, string? contactEmail, string? contactPhone, CancellationToken ct = default);
        Task<Result> ToggleClientActiveAsync(Guid clientId, CancellationToken ct = default);

        Task<Result<Guid>> LinkClientToSystemAsync(Guid clientId, Guid systemId, DateTime? contractStartDate = null, DateTime? contractEndDate = null, int? maxUsersQuota = null, CancellationToken ct = default);
        Task<Result> UnlinkClientFromSystemAsync(Guid clientId, Guid systemId, CancellationToken ct = default);
        Task<Result> ToggleClientSystemActiveAsync(Guid clientId, Guid systemId, CancellationToken ct = default);
    }
}
