using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IdentityAuthLayer.Domain.Enums;

namespace IdentityAuthLayer.Domain.Entities
{
    /// <summary>
    /// يوزر تابع لعميل معين (Admin / GroupAdmin / User).
    /// الصلاحيات الفعلية بتيجي من الـ Roles المربوطة بيه، وكل Role مربوط بسيستم محدد.
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = default!;

        public string Username { get; set; } = default!;

        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string Phone { get; set; } = default!;

        /// <summary>فاضية لحد ما اليوزر يعمل VerifyAccount + CreatePassword (نفس فكرة CarCare).</summary>
        public string? PasswordHash { get; set; }

        public UserType UserType { get; set; } = UserType.User;

        public bool IsActive { get; set; } = true;

        public bool IsEmailVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>عدد محاولات تسجيل الدخول الفاشلة المتتالية.</summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>وقت انتهاء حظر تسجيل الدخول المؤقت.</summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>كود التحقق من الحساب (Register -> VerifyAccount). يتم تخزينه كـ HMAC وليس Plain Text.</summary>
        public VerificationCode? EmailVerification { get; set; } = new();

        /// <summary>كود إعادة تعيين كلمة المرور (ForgotPassword -> VerifyResetCode -> ResetPassword).</summary>
        public VerificationCode? PasswordReset { get; set; } = new();

        /// <summary>بيانات الـ Refresh Token (Login -> RefreshToken).</summary>
        public RefreshTokenInfo? RefreshTokenInfo { get; set; } = new();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    /// <summary>Owned type - بيتسجل مرتين على اليوزر (EmailVerification و PasswordReset) بأعمدة مختلفة.</summary>
    public class VerificationCode
    {
        public string? Code { get; set; }
        public string? PreviousCode { get; set; }
        public DateTime? CodeExpiry { get; set; }
        public DateTime? LastSentAt { get; set; }
        public bool IsVerified { get; set; } = false;
    }

    /// <summary>Owned type - بيانات الـ Refresh Token بتاعة اليوزر.</summary>
    public class RefreshTokenInfo
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
