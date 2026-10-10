using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityAuthLayer.API.Controllers.Base;
using IdentityAuthLayer.API.Routes;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Application.Users.Commands.CreateUser;
using IdentityAuthLayer.Application.Users.Commands.AssignRoleToUser;
using IdentityAuthLayer.Application.Users.Queries.GetUserById;
using IdentityAuthLayer.Application.Users.Queries.GetUserPermissions;
using IdentityAuthLayer.Application.Users.Queries.CheckUserPermission;
using IdentityAuthLayer.Domain.Enums;
using IdentityAuthLayer.Infrastructure.Persistence;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.API.Controllers
{
    public record UpdateUserRequest(string FirstName, string LastName, string Phone);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    /// <summary>
    /// إدارة المستخدمين (Admin/GroupAdmin/User) + إدارة الملف الشخصي والصلاحيات عبر الأنظمة.
    /// </summary>
    public class UsersController : ApplicationBaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        private string? CurrentUserType => User.FindFirst("userType")?.Value;

        private Guid? CurrentClientId
        {
            get
            {
                var val = User.FindFirst("clientId")?.Value;
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        private bool IsAdmin => string.Equals(CurrentUserType, "Admin", StringComparison.OrdinalIgnoreCase);
        private bool IsGroupAdmin => string.Equals(CurrentUserType, "GroupAdmin", StringComparison.OrdinalIgnoreCase);

        /// <summary>عرض المستخدمين (محدد بجهة العمل للـ GroupAdmin أو بفلترة اختيارية للأدمن العام).</summary>
        [HttpGet(UsersRoutes.Create)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? clientId, [FromServices] ApplicationDbContext db)
        {
            var query = db.Users.AsQueryable();

            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                // عزل المستأجرين: مدير الجهة يرى فقط مستخدمي شركته
                query = query.Where(u => u.ClientId == CurrentClientId.Value);
            }
            else if (clientId.HasValue && clientId.Value != Guid.Empty)
            {
                query = query.Where(u => u.ClientId == clientId.Value);
            }

            var list = await query
                .Include(u => u.Client)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.System)
                .Select(u => new
                {
                    u.Id,
                    u.ClientId,
                    ClientName = u.Client.Name,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    UserType = u.UserType.ToString(),
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => new
                    {
                        ur.Role.Id,
                        ur.Role.SystemId,
                        SystemCode = ur.Role.System.Code,
                        SystemName = ur.Role.System.Name,
                        RoleName = ur.Role.Name
                    })
                })
                .ToListAsync();

            return BaseResponse(list);
        }

        /// <summary>عرض الملف الشخصي للمستخدم الحالي مسجل الدخول مع كافة أدواره وأنظمته.</summary>
        [HttpGet(UsersRoutes.Me)]
        public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        {
            if (!CurrentUserId.HasValue)
                return BaseResponse(Result.Failure("يرجى إرسال رمز الدخول (Token) لمعرفة الملف الشخصي الحالي"));

            return BaseResponse(await _userService.GetMyProfileAsync(CurrentUserId.Value, ct));
        }

        /// <summary>تغيير كلمة المرور للمستخدم الحالي مسجل الدخول.</summary>
        [HttpPost(UsersRoutes.ChangePassword)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            if (!CurrentUserId.HasValue)
                return BaseResponse(Result.Failure("يرجى إرسال رمز الدخول (Token) لتغيير كلمة المرور"));

            return BaseResponse(await _userService.ChangePasswordAsync(CurrentUserId.Value, request.CurrentPassword, request.NewPassword, ct));
        }

        /// <summary>عرض بيانات مستخدم محدد بواسطة الـ userId.</summary>
        [HttpGet(UsersRoutes.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid userId, [FromServices] ApplicationDbContext db, CancellationToken ct)
        {
            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                var targetUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (targetUser is null)
                    return BaseResponse(Result.Failure("المستخدم غير موجود"));
                if (targetUser.ClientId != CurrentClientId.Value)
                    return BaseResponse(Result.Failure("غير مصرح لك بالوصول لمستخدم تابع لجهة أخرى"));
            }

            return BaseResponse(await _userService.GetMyProfileAsync(userId, ct));
        }

        /// <summary>إضافة يوزر.</summary>
        [HttpPost(UsersRoutes.Create)]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                if (command.UserType == UserType.Admin)
                    return BaseResponse(Result.Failure("غير مصرح لمدير الجهة بإنشاء حساب مسؤول نظام"));

                // فرض إضافة المستخدم لنفس شركة الـ GroupAdmin
                command = command with { ClientId = CurrentClientId.Value };
            }

            return BaseResponse(await _mediator.Send(command));
        }

        /// <summary>تعديل بيانات يوزر محدد (الاسم ورقم الهاتف).</summary>
        [HttpPut(UsersRoutes.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid userId, [FromBody] UpdateUserRequest request, [FromServices] ApplicationDbContext db, CancellationToken ct)
        {
            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                var targetUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (targetUser is null)
                    return BaseResponse(Result.Failure("المستخدم غير موجود"));
                if (targetUser.ClientId != CurrentClientId.Value)
                    return BaseResponse(Result.Failure("غير مصرح لك بتعديل مستخدم تابع لجهة أخرى"));
            }

            return BaseResponse(await _userService.UpdateUserAsync(userId, request.FirstName, request.LastName, request.Phone, ct));
        }

        /// <summary>تفعيل أو تجميد حساب يوزر محدد.</summary>
        [HttpPost(UsersRoutes.ToggleActive)]
        public async Task<IActionResult> ToggleActive([FromRoute] Guid userId, [FromServices] ApplicationDbContext db, CancellationToken ct)
        {
            if (CurrentUserId.HasValue && userId == CurrentUserId.Value)
                return BaseResponse(Result.Failure("لا يمكن تجميد حسابك الحالي"));

            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                var targetUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (targetUser is null)
                    return BaseResponse(Result.Failure("المستخدم غير موجود"));
                if (targetUser.ClientId != CurrentClientId.Value)
                    return BaseResponse(Result.Failure("غير مصرح لك بتعديل مستخدم تابع لجهة أخرى"));
            }

            return BaseResponse(await _userService.ToggleUserActiveAsync(userId, ct));
        }

        /// <summary>ربط رول بيوزر (يُشترط تعاقد جهة اليوزر على سيستم الرول وعدم تجاوز كوتا المستخدمين).</summary>
        [HttpPost(UsersRoutes.AssignRole)]
        public async Task<IActionResult> AssignRole([FromRoute] Guid userId, [FromBody] Guid roleId, [FromServices] ApplicationDbContext db, CancellationToken ct)
        {
            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                var targetUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (targetUser is null)
                    return BaseResponse(Result.Failure("المستخدم غير موجود"));
                if (targetUser.ClientId != CurrentClientId.Value)
                    return BaseResponse(Result.Failure("غير مصرح لك بربط أدوار لمستخدم تابع لجهة أخرى"));
            }

            return BaseResponse(await _mediator.Send(new AssignRoleToUserCommand(userId, roleId)));
        }

        /// <summary>إلغاء ربط رول من يوزر.</summary>
        [HttpDelete(UsersRoutes.RemoveRole)]
        public async Task<IActionResult> RemoveRole([FromRoute] Guid userId, [FromRoute] Guid roleId, [FromServices] ApplicationDbContext db, CancellationToken ct)
        {
            if (IsGroupAdmin && CurrentClientId.HasValue)
            {
                var targetUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
                if (targetUser is null)
                    return BaseResponse(Result.Failure("المستخدم غير موجود"));
                if (targetUser.ClientId != CurrentClientId.Value)
                    return BaseResponse(Result.Failure("غير مصرح لك بتعديل أدوار مستخدم تابع لجهة أخرى"));
            }

            return BaseResponse(await _userService.RemoveRoleFromUserAsync(userId, roleId, ct));
        }

        /// <summary>
        /// يُستخدم من أي سيستم فرعي: جلب كل صلاحيات اليوزر على سيستم محدد (systemId).
        /// </summary>
        [HttpGet(UsersRoutes.GetPermissions)]
        public async Task<IActionResult> GetPermissions([FromRoute] Guid userId, [FromQuery] Guid systemId)
            => BaseResponse(await _mediator.Send(new GetUserPermissionsQuery(userId, systemId)));

        /// <summary>
        /// يُستخدم من أي سيستم فرعي: فحص هل يمتلك اليوزر صلاحية معينة على هذا السيستم.
        /// </summary>
        [HttpGet(UsersRoutes.CheckPermission)]
        public async Task<IActionResult> CheckPermission(
            [FromRoute] Guid userId, [FromQuery] Guid systemId, [FromQuery] string permissionCode)
            => BaseResponse(await _mediator.Send(new CheckUserPermissionQuery(userId, systemId, permissionCode)));
    }
}
