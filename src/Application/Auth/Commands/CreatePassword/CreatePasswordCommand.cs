using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.CreatePassword
{
    public record CreatePasswordCommand(string Email, string Password) : IRequest<Result>;

    public class CreatePasswordCommandValidator : AbstractValidator<CreatePasswordCommand>
    {
        public CreatePasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty().MinimumLength(8).MaximumLength(100)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
                .WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير وصغير ورقم على الأقل");
        }
    }

    public class CreatePasswordCommandHandler : IRequestHandler<CreatePasswordCommand, Result>
    {
        private readonly IAuthService _authService;

        public CreatePasswordCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(CreatePasswordCommand request, CancellationToken cancellationToken) =>
            _authService.CreatePasswordAsync(request.Email, request.Password, cancellationToken);
    }
}
