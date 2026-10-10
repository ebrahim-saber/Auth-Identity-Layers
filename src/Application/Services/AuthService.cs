using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IdentityAuthLayer.Application.Common.Interfaces;
using IdentityAuthLayer.Application.Common.Interfaces.Repositories;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Entities;
using IdentityAuthLayer.Domain.Enums;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IAuthSecurity _authSecurity;
        private const int VerificationCodeExpiryMinutes = 15;
        private const int MaxFailedLoginAttempts = 5;
        private const int LockoutMinutes = 15;
        private const int RefreshTokenExpiryDays = 7;

        public AuthService(IUnitOfWork uow, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService,
            IEmailService emailService, IAuthSecurity authSecurity)
        {
            _uow = uow;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _authSecurity = authSecurity;
        }

        public async Task<Result> RegisterAsync(Guid clientId, string username, string firstName, string lastName, string email, string phone, CancellationToken ct = default)
        {
            if (!await _uow.Clients.ExistsAsync(clientId, ct)) return Result.Failure("العميل غير موجود.");
            if (await _uow.Users.ExistsByEmailAsync(email, ct)) return Result.Failure("البريد الإلكتروني مستخدم بالفعل.");
            if (await _uow.Users.ExistsByUsernameAsync(username, ct)) return Result.Failure("اسم المستخدم مستخدم بالفعل.");

            var code = GenerateNumericCode();
            var user = new User
            {
                ClientId = clientId, Username = username, FirstName = firstName, LastName = lastName,
                Email = email, Phone = phone, PasswordHash = null, UserType = UserType.User,
                IsActive = true, IsEmailVerified = false,
                EmailVerification = new VerificationCode
                {
                    Code = string.Empty,
                    PreviousCode = null,
                    CodeExpiry = DateTime.UtcNow.AddMinutes(VerificationCodeExpiryMinutes),
                    LastSentAt = DateTime.UtcNow,
                    IsVerified = false
                }
            };
            user.EmailVerification!.Code = _authSecurity.HashVerificationCode(user.Id, code, "email-verification");

            await _uow.Users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);
            await _emailService.SendVerificationCodeAsync(email, code, $"{firstName} {lastName}".Trim());
            return Result.Success("تم إنشاء الحساب، تم إرسال كود التفعيل إلى بريدك الإلكتروني");
        }

        public async Task<Result> VerifyAccountAsync(string email, string verificationCode, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            if (user is null) return Result.Failure("لم يتم العثور على مستخدم بهذا البريد الإلكتروني.");
            if (user.IsEmailVerified) return Result.Failure("الحساب مُفعل بالفعل.");
            if (user.EmailVerification?.CodeExpiry is null || user.EmailVerification.CodeExpiry <= DateTime.UtcNow)
                return Result.Failure("انتهت صلاحية كود التحقق.");

            var cleanCode = verificationCode?.Trim() ?? string.Empty;
            var isCurrentMatch = !string.IsNullOrWhiteSpace(user.EmailVerification.Code) &&
                _authSecurity.VerifyVerificationCode(user.Id, cleanCode, "email-verification", user.EmailVerification.Code);

            var isPreviousMatch = !string.IsNullOrWhiteSpace(user.EmailVerification.PreviousCode) &&
                _authSecurity.VerifyVerificationCode(user.Id, cleanCode, "email-verification", user.EmailVerification.PreviousCode);

            if (!isCurrentMatch && !isPreviousMatch)
                return Result.Failure("كود التحقق غير صحيح.");

            user.IsEmailVerified = true;
            user.EmailVerification.IsVerified = true;
            user.EmailVerification.Code = null;
            user.EmailVerification.PreviousCode = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تأكيد الحساب بنجاح، يمكنك الآن إنشاء كلمة مرور");
        }

        public async Task<Result> ResendVerificationCodeAsync(string email, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            if (user is null) return Result.Failure("لم يتم العثور على مستخدم بهذا البريد الإلكتروني.");
            if (user.IsEmailVerified) return Result.Failure("الحساب مُفعل بالفعل.");

            // Cooldown: 60 seconds
            if (user.EmailVerification?.LastSentAt != null &&
                user.EmailVerification.LastSentAt.Value.AddSeconds(60) > DateTime.UtcNow)
            {
                var remaining = (int)Math.Ceiling((user.EmailVerification.LastSentAt.Value.AddSeconds(60) - DateTime.UtcNow).TotalSeconds);
                return Result.Failure($"تم إرسال كود التحقق بالفعل. يرجى الانتظار {remaining} ثانية قبل طلب كود جديد.");
            }

            var code = GenerateNumericCode();
            user.EmailVerification = new VerificationCode
            {
                PreviousCode = user.EmailVerification?.Code,
                Code = _authSecurity.HashVerificationCode(user.Id, code, "email-verification"),
                CodeExpiry = DateTime.UtcNow.AddMinutes(VerificationCodeExpiryMinutes),
                LastSentAt = DateTime.UtcNow,
                IsVerified = false
            };
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            await _emailService.SendVerificationCodeAsync(email, code, $"{user.FirstName} {user.LastName}".Trim());
            return Result.Success("تم إعادة إرسال كود التحقق بنجاح");
        }

        public async Task<Result> CreatePasswordAsync(string email, string password, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            if (user is null) return Result.Failure("لم يتم العثور على مستخدم بهذا البريد الإلكتروني.");
            if (!user.IsEmailVerified) return Result.Failure("يجب تأكيد البريد الإلكتروني أولاً.");
            if (user.EmailVerification?.IsVerified != true) return Result.Failure("يجب التحقق من كود التأكيد أولاً.");
            if (!string.IsNullOrWhiteSpace(user.PasswordHash)) return Result.Failure("كلمة المرور موجودة بالفعل.");

            user.PasswordHash = _passwordHasher.Hash(password);
            user.UpdatedAt = DateTime.UtcNow;
            user.EmailVerification.Code = null;
            user.EmailVerification.PreviousCode = null;
            user.EmailVerification.CodeExpiry = null;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم إنشاء كلمة المرور بنجاح");
        }

        public async Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            if (user is null) return Result.Failure("لم يتم العثور على مستخدم بهذا البريد الإلكتروني.");
            if (!user.IsActive) return Result.Failure("الحساب غير مفعل.");
            if (!user.IsEmailVerified) return Result.Failure("البريد الإلكتروني غير مُؤكد. يرجى تأكيد بريدك الإلكتروني أولاً.");

            // Cooldown: 60 seconds
            if (user.PasswordReset?.LastSentAt != null &&
                user.PasswordReset.LastSentAt.Value.AddSeconds(60) > DateTime.UtcNow)
            {
                var remaining = (int)Math.Ceiling((user.PasswordReset.LastSentAt.Value.AddSeconds(60) - DateTime.UtcNow).TotalSeconds);
                return Result.Failure($"تم إرسال كود إعادة التعيين بالفعل. يرجى الانتظار {remaining} ثانية قبل طلب كود جديد.");
            }

            var code = GenerateNumericCode();
            user.PasswordReset = new VerificationCode
            {
                PreviousCode = user.PasswordReset?.Code,
                Code = _authSecurity.HashVerificationCode(user.Id, code, "password-reset"),
                CodeExpiry = DateTime.UtcNow.AddMinutes(VerificationCodeExpiryMinutes),
                LastSentAt = DateTime.UtcNow,
                IsVerified = false
            };
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            await _emailService.SendResetPasswordCodeAsync(email, code, $"{user.FirstName} {user.LastName}".Trim());
            return Result.Success("تم إرسال كود إعادة تعيين كلمة المرور إلى بريدك الإلكتروني");
        }

        public async Task<Result> VerifyResetCodeAsync(string email, string code, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            if (user is null) return Result.Failure("لم يتم العثور على مستخدم بهذا البريد الإلكتروني.");
            if (user.PasswordReset?.Code is null && user.PasswordReset?.PreviousCode is null) return Result.Failure("لم يتم طلب إعادة تعيين كلمة المرور.");
            if (user.PasswordReset.CodeExpiry is null || user.PasswordReset.CodeExpiry <= DateTime.UtcNow)
                return Result.Failure("انتهت صلاحية كود إعادة التعيين، يرجى طلب كود جديد.");

            var cleanCode = code?.Trim() ?? string.Empty;
            var isCurrentMatch = !string.IsNullOrWhiteSpace(user.PasswordReset.Code) &&
                _authSecurity.VerifyVerificationCode(user.Id, cleanCode, "password-reset", user.PasswordReset.Code);

            var isPreviousMatch = !string.IsNullOrWhiteSpace(user.PasswordReset.PreviousCode) &&
                _authSecurity.VerifyVerificationCode(user.Id, cleanCode, "password-reset", user.PasswordReset.PreviousCode);

            if (!isCurrentMatch && !isPreviousMatch)
                return Result.Failure("كود إعادة التعيين غير صحيح.");

            user.PasswordReset.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم التحقق من الكود بنجاح، يمكنك الآن تعيين كلمة مرور جديدة");
        }

        public async Task<Result> ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            if (user is null) return Result.Failure("لم يتم العثور على مستخدم بهذا البريد الإلكتروني.");
            if (user.PasswordReset?.Code is null && user.PasswordReset?.PreviousCode is null) return Result.Failure("لم يتم طلب إعادة تعيين كلمة المرور.");
            if (user.PasswordReset.CodeExpiry is null || user.PasswordReset.CodeExpiry <= DateTime.UtcNow)
                return Result.Failure("انتهت صلاحية كود إعادة التعيين، يرجى طلب كود جديد.");
            if (!user.PasswordReset.IsVerified) return Result.Failure("يجب التحقق من كود إعادة التعيين أولاً.");

            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.PasswordReset.Code = null;
            user.PasswordReset.PreviousCode = null;
            user.PasswordReset.CodeExpiry = null;
            user.PasswordReset.IsVerified = false;
            user.UpdatedAt = DateTime.UtcNow;
            if (user.RefreshTokenInfo is not null)
            {
                user.RefreshTokenInfo.RefreshToken = null; user.RefreshTokenInfo.RefreshTokenExpiryTime = null;
            }
            await _uow.SaveChangesAsync(ct);
            await _emailService.SendPasswordChangedNotificationAsync(email, $"{user.FirstName} {user.LastName}".Trim());
            return Result.Success("تم تغيير كلمة المرور بنجاح");
        }

        public async Task<Result<AuthTokensDto>> LoginAsync(string username, string password, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByUsernameAsync(username, ct);
            if (user is null && username.Contains("@"))
            {
                user = await _uow.Users.GetByEmailAsync(username, ct);
            }
            if (user is null) return Result.Failure<AuthTokensDto>("اسم المستخدم أو كلمة المرور غير صحيحة.");
            if (!user.IsActive) return Result.Failure<AuthTokensDto>("الحساب غير مفعل.");

            var client = await _uow.Clients.GetByIdAsync(user.ClientId, ct);
            if (client is not null && !client.IsActive)
                return Result.Failure<AuthTokensDto>("تم تجميد حساب الشركة/العميل. يرجى التواصل مع إدارة النظام.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                return Result.Failure<AuthTokensDto>("تم حظر تسجيل الدخول مؤقتًا بسبب تكرار المحاولات الفاشلة. حاول لاحقًا.");
            if (user.LockoutEnd.HasValue) { user.LockoutEnd = null; user.FailedLoginAttempts = 0; }

            if (!user.IsEmailVerified) return Result.Failure<AuthTokensDto>("يجب تأكيد البريد الإلكتروني أولاً.");
            if (string.IsNullOrWhiteSpace(user.PasswordHash)) return Result.Failure<AuthTokensDto>("يجب إنشاء كلمة مرور أولاً.");

            if (!_passwordHasher.Verify(password, user.PasswordHash!))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    user.FailedLoginAttempts = 0;
                }
                await _uow.SaveChangesAsync(ct);
                return Result.Failure<AuthTokensDto>("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            user.FailedLoginAttempts = 0; user.LockoutEnd = null;
            var dto = await IssueTokensAsync(user, ct);
            return Result.Success(dto, "تم تسجيل الدخول بنجاح");
        }

        public async Task<Result<AuthTokensDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByRefreshTokenAsync(refreshToken, ct);
            var hash = _authSecurity.HashRefreshToken(refreshToken);
            var isValid = user is not null && user.RefreshTokenInfo?.RefreshToken == hash &&
                          user.RefreshTokenInfo.RefreshTokenExpiryTime > DateTime.UtcNow && user.IsActive;
            if (!isValid) return Result.Failure<AuthTokensDto>("الرمز المميز غير صحيح أو منتهي الصلاحية.");

            var dto = await IssueTokensAsync(user!, ct);
            return Result.Success(dto, "تم تحديث الرمز المميز بنجاح");
        }

        public async Task<Result> LogoutAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct);
            if (user is null) return Result.Failure("المستخدم غير موجود.");
            if (user.RefreshTokenInfo is not null)
            {
                user.RefreshTokenInfo.RefreshToken = null;
                user.RefreshTokenInfo.RefreshTokenExpiryTime = null;
                user.RefreshTokenInfo.LastLoginDate = null;
            }
            user.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return Result.Success("تم تسجيل الخروج بنجاح");
        }

        public async Task<Result<EmailCheckResultDto>> CheckEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByEmailAsync(email, ct);
            var dto = user is null ? new EmailCheckResultDto(false, false, false) :
                new EmailCheckResultDto(true, user.IsEmailVerified, !string.IsNullOrWhiteSpace(user.PasswordHash));
            return Result.Success(dto);
        }

        private async Task<AuthTokensDto> IssueTokensAsync(User user, CancellationToken ct)
        {
            var accessToken = _jwtTokenService.GenerateToken(user.Id, user.ClientId, user.Username, user.UserType.ToString());
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(60);
            user.RefreshTokenInfo ??= new RefreshTokenInfo();
            user.RefreshTokenInfo.RefreshToken = _authSecurity.HashRefreshToken(refreshToken);
            user.RefreshTokenInfo.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
            user.RefreshTokenInfo.LastLoginDate = DateTime.UtcNow;
            await _uow.SaveChangesAsync(ct);
            return new AuthTokensDto(accessToken, refreshToken, expiresAt, user.Id, user.ClientId, user.Username, user.UserType.ToString());
        }

        private static string GenerateNumericCode() => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}
