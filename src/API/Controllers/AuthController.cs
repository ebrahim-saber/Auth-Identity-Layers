using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using IdentityAuthLayer.API.Controllers.Base;
using IdentityAuthLayer.API.Routes;
using IdentityAuthLayer.Application.Auth.Commands.Login;
using IdentityAuthLayer.Application.Auth.Commands.Register;
using IdentityAuthLayer.Application.Auth.Commands.VerifyAccount;
using IdentityAuthLayer.Application.Auth.Commands.ResendVerificationCode;
using IdentityAuthLayer.Application.Auth.Commands.CreatePassword;
using IdentityAuthLayer.Application.Auth.Commands.RefreshToken;
using IdentityAuthLayer.Application.Auth.Commands.ForgotPassword;
using IdentityAuthLayer.Application.Auth.Commands.VerifyResetCode;
using IdentityAuthLayer.Application.Auth.Commands.ResetPassword;
using IdentityAuthLayer.Application.Auth.Commands.Logout;
using IdentityAuthLayer.Application.Auth.Queries.CheckEmail;

namespace IdentityAuthLayer.API.Controllers
{
    /// <summary>
    /// مركزي - أي سيستم فرعي (تراكينج، حج، معاقين) متعملوش Login/Register خاص بيه،
    /// بيستخدم نفس التوكن اللي بيرجع من هنا.
    /// كل endpoint عام عليه [AllowAnonymous] بشكل صريح، بينما Logout يتطلب JWT.
    /// </summary>
    [EnableRateLimiting("auth")]
    public class AuthController : ApplicationBaseController
    {
        // 1. تسجيل حساب جديد - بيبعت كود تفعيل على الإيميل
        [AllowAnonymous]
        [HttpPost(AuthRoutes.Register)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 2. تفعيل الحساب بالكود اللي وصل على الإيميل
        [AllowAnonymous]
        [HttpPost(AuthRoutes.VerifyAccount)]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 3. إعادة إرسال كود التفعيل
        [AllowAnonymous]
        [HttpPost(AuthRoutes.ResendVerificationCode)]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 4. إنشاء كلمة مرور بعد تفعيل الحساب
        [AllowAnonymous]
        [HttpPost(AuthRoutes.CreatePassword)]
        public async Task<IActionResult> CreatePassword([FromBody] CreatePasswordCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 5. تسجيل الدخول - بيرجع Access Token + Refresh Token
        [AllowAnonymous]
        [HttpPost(AuthRoutes.Login)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 6. تجديد الـ Access Token عن طريق الـ Refresh Token
        [AllowAnonymous]
        [HttpPost(AuthRoutes.RefreshToken)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 7. طلب إعادة تعيين كلمة المرور (كود عبر الإيميل)
        [AllowAnonymous]
        [HttpPost(AuthRoutes.ForgotPassword)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 8. التحقق من كود إعادة التعيين
        [AllowAnonymous]
        [HttpPost(AuthRoutes.VerifyResetCode)]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 9. تعيين كلمة مرور جديدة بعد التحقق من الكود
        [AllowAnonymous]
        [HttpPost(AuthRoutes.ResetPassword)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
            => BaseResponse(await _mediator.Send(command));

        // 10. تسجيل الخروج - إبطال Refresh Token الحالي
        [Authorize]
        [DisableRateLimiting]
        [HttpPost(AuthRoutes.Logout)]
        public async Task<IActionResult> Logout()
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(rawUserId, out var userId)) return Unauthorized();
            return BaseResponse(await _mediator.Send(new LogoutCommand(userId)));
        }

        // 11. فحص إيميل
        [AllowAnonymous]
        [HttpGet(AuthRoutes.CheckEmail)]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
            => BaseResponse(await _mediator.Send(new CheckEmailQuery(email)));
    }
}
