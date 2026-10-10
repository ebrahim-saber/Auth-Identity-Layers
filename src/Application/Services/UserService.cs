using IdentityAuthLayer.Application.Common.Interfaces;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Entities;
using IdentityAuthLayer.Domain.Enums;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUnitOfWork uow, IPasswordHasher passwordHasher)
        {
            _uow = uow;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<Guid>> CreateUserAsync(Guid clientId, string username, string firstName, string lastName,
            string email, string phone, string password, UserType userType, CancellationToken ct = default)
        {
            if (!await _uow.Clients.ExistsAsync(clientId, ct))
                return Result.Failure<Guid>("العميل غير موجود.");

            if (await _uow.Users.ExistsByUsernameAsync(username, ct))
                return Result.Failure<Guid>("اسم المستخدم مستخدم بالفعل.");

            if (await _uow.Users.ExistsByEmailAsync(email, ct))
                return Result.Failure<Guid>("البريد الإلكتروني مستخدم بالفعل.");

            var user = new User
            {
                ClientId = clientId,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                PasswordHash = _passwordHasher.Hash(password),
                UserType = userType,
                IsEmailVerified = true // الأدمن هو اللي بيضيفه مباشرة، فمفيش داعي لـ verification flow
            };

            await _uow.Users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(user.Id, "تم إنشاء اليوزر بنجاح");
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null)
                return Result.Failure<UserDto>("اليوزر غير موجود.");

            var dto = new UserDto(user.Id, user.ClientId, user.Username, user.FirstName, user.LastName,
                user.Email, user.Phone, user.UserType.ToString(), user.IsActive, user.IsEmailVerified, user.CreatedAt);

            return Result.Success(dto);
        }

        public async Task<Result<UserProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetWithRolesAsync(userId, ct);
            if (user is null)
                return Result.Failure<UserProfileDto>("اليوزر غير موجود.");

            var roles = user.UserRoles.Select(ur => new UserRoleSummaryDto(
                ur.Role.Id,
                ur.Role.Name,
                ur.Role.SystemId,
                ur.Role.System.Code,
                ur.Role.System.Name
            )).ToList();

            var dto = new UserProfileDto(user.Id, user.ClientId, user.Client?.Name ?? string.Empty,
                user.Username, user.FirstName, user.LastName, user.Email, user.Phone,
                user.UserType.ToString(), user.IsActive, user.IsEmailVerified, user.CreatedAt, roles);

            return Result.Success(dto);
        }

        public async Task<Result> UpdateUserAsync(Guid userId, string firstName, string lastName, string phone, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null)
                return Result.Failure("اليوزر غير موجود.");

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Phone = phone;
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تحديث بيانات المستخدم بنجاح");
        }

        public async Task<Result> ToggleUserActiveAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null)
                return Result.Failure("اليوزر غير موجود.");

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            var statusStr = user.IsActive ? "تنشيط" : "تعطيل";
            return Result.Success($"تم {statusStr} حساب المستخدم بنجاح");
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null)
                return Result.Failure("اليوزر غير موجود.");

            if (string.IsNullOrWhiteSpace(user.PasswordHash) || !_passwordHasher.Verify(currentPassword, user.PasswordHash))
                return Result.Failure("كلمة المرور الحالية غير صحيحة.");

            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تغيير كلمة المرور بنجاح");
        }

        public async Task<Result<Guid>> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null)
                return Result.Failure<Guid>("اليوزر غير موجود.");

            var role = await _uow.Roles.GetByIdAsync(roleId, ct);
            if (role is null)
                return Result.Failure<Guid>("الرول غير موجود.");

            // ====== القاعدة: عميل اليوزر لازم يكون متعاقد على سيستم الرول وساري الصلاحية ======
            var clientSystem = await _uow.ClientSystems.GetByClientAndSystemAsync(user.ClientId, role.SystemId, ct);
            if (clientSystem is null || !clientSystem.IsActive)
                return Result.Failure<Guid>("عميل اليوزر غير متعاقد على السيستم الخاص بهذا الرول أو تم تجميد الاشتراك.");

            if (clientSystem.ContractEndDate.HasValue && clientSystem.ContractEndDate.Value <= DateTime.UtcNow)
                return Result.Failure<Guid>("انتهت فترة اشتراك العميل في هذا النظام. يرجى تجديد العقد.");

            if (await _uow.UserRoles.ExistsAsync(userId, roleId, ct))
                return Result.Failure<Guid>("الرول مضاف بالفعل لهذا اليوزر.");

            // فحص سقف المستخدمين (MaxUsersQuota)
            if (clientSystem.MaxUsersQuota.HasValue)
            {
                var currentUsersCount = await _uow.UserRoles.CountDistinctUsersOnSystemAsync(user.ClientId, role.SystemId, ct);
                var existingRolesOnSystem = await _uow.UserRoles.GetUserPermissionsAsync(userId, role.SystemId, ct);
                if (existingRolesOnSystem.Count == 0 && currentUsersCount >= clientSystem.MaxUsersQuota.Value)
                {
                    return Result.Failure<Guid>($"تم الوصول للحد الأقصى لعدد المستخدمين المسموح به لهذا النظام ({clientSystem.MaxUsersQuota.Value} مستخدم).");
                }
            }

            var userRole = new UserRole { UserId = userId, RoleId = roleId };

            await _uow.UserRoles.AddAsync(userRole, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(userRole.Id, "تم إضافة الرول لليوزر بنجاح");
        }

        public async Task<Result> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        {
            var userRole = await _uow.UserRoles.GetByUserAndRoleAsync(userId, roleId, ct);
            if (userRole is null)
                return Result.Failure("هذا الرول غير معين لهذا المستخدم.");

            _uow.UserRoles.Remove(userRole);
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم سحب الرول من المستخدم بنجاح");
        }

        public async Task<Result<List<UserPermissionDto>>> GetUserPermissionsAsync(Guid userId, Guid systemId, CancellationToken ct = default)
        {
            var records = await _uow.UserRoles.GetUserPermissionsAsync(userId, systemId, ct);

            var dtos = records
                .Select(r => new UserPermissionDto(r.RoleName, r.PermissionCode, r.PermissionName))
                .ToList();

            return Result.Success(dtos);
        }

        public async Task<Result<bool>> CheckUserPermissionAsync(Guid userId, Guid systemId, string permissionCode, CancellationToken ct = default)
        {
            var hasPermission = await _uow.UserRoles.HasPermissionAsync(userId, systemId, permissionCode, ct);
            return Result.Success(hasPermission);
        }
    }
}
