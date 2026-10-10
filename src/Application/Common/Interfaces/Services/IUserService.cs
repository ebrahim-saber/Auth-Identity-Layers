using IdentityAuthLayer.Domain.Enums;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Common.Interfaces.Services
{
    public record UserPermissionDto(string RoleName, string PermissionCode, string PermissionName);

    public record UserDto(Guid Id, Guid ClientId, string Username, string FirstName, string LastName,
        string Email, string Phone, string UserType, bool IsActive, bool IsEmailVerified, DateTime CreatedAt);

    public record UserRoleSummaryDto(Guid RoleId, string RoleName, Guid SystemId, string SystemCode, string SystemName);

    public record UserProfileDto(Guid Id, Guid ClientId, string ClientName, string Username, string FirstName, string LastName,
        string Email, string Phone, string UserType, bool IsActive, bool IsEmailVerified, DateTime CreatedAt, List<UserRoleSummaryDto> Roles);

    public interface IUserService
    {
        /// <summary>إضافة يوزر مباشرة بمعرفة الأدمن (بكلمة سر جاهزة) - مختلف عن Register اللي فيه verification flow.</summary>
        Task<Result<Guid>> CreateUserAsync(Guid clientId, string username, string firstName, string lastName,
            string email, string phone, string password, UserType userType, CancellationToken ct = default);

        Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);

        Task<Result<UserProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

        Task<Result> UpdateUserAsync(Guid userId, string firstName, string lastName, string phone, CancellationToken ct = default);

        Task<Result> ToggleUserActiveAsync(Guid userId, CancellationToken ct = default);

        Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);

        /// <summary>القاعدة هنا: عميل اليوزر لازم يكون متعاقد على سيستم الرول قبل ما نديله إياه مع فحص الكوتة.</summary>
        Task<Result<Guid>> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);

        Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);

        Task<Result<List<UserPermissionDto>>> GetUserPermissionsAsync(Guid userId, Guid systemId, CancellationToken ct = default);

        Task<Result<bool>> CheckUserPermissionAsync(Guid userId, Guid systemId, string permissionCode, CancellationToken ct = default);
    }
}
