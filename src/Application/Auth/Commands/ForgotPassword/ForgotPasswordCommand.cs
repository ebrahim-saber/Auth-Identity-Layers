using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result>;

    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly IAuthService _authService;

        public ForgotPasswordCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken) =>
            _authService.ForgotPasswordAsync(request.Email, cancellationToken);
    }
}
