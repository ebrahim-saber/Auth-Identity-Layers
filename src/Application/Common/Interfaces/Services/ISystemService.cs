using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Common.Interfaces.Services
{
    public record SystemDetailsDto(Guid Id, string Code, string Name, string? Description, string ApiKey, bool IsActive, DateTime CreatedAt);

    public interface ISystemService
    {
        Task<Result<Guid>> CreateSystemAsync(string code, string name, string? description, CancellationToken ct = default);
        Task<Result<SystemDetailsDto>> GetSystemByIdAsync(Guid systemId, CancellationToken ct = default);
        Task<Result> UpdateSystemAsync(Guid systemId, string name, string? description, CancellationToken ct = default);
        Task<Result> ToggleSystemActiveAsync(Guid systemId, CancellationToken ct = default);
    }
}
