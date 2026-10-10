using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Email, string NewPassword) : IRequest<Result>;

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.NewPassword)
                .NotEmpty().MinimumLength(8).MaximumLength(100)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
                .WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير وصغير ورقم على الأقل");
        }
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IAuthService _authService;

        public ResetPasswordCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken) =>
            _authService.ResetPasswordAsync(request.Email, request.NewPassword, cancellationToken);
    }
}
