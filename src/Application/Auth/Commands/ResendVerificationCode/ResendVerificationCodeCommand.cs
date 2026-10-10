using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.ResendVerificationCode
{
    public record ResendVerificationCodeCommand(string Email) : IRequest<Result>;

    public class ResendVerificationCodeCommandValidator : AbstractValidator<ResendVerificationCodeCommand>
    {
        public ResendVerificationCodeCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class ResendVerificationCodeCommandHandler : IRequestHandler<ResendVerificationCodeCommand, Result>
    {
        private readonly IAuthService _authService;

        public ResendVerificationCodeCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(ResendVerificationCodeCommand request, CancellationToken cancellationToken) =>
            _authService.ResendVerificationCodeAsync(request.Email, cancellationToken);
    }
}
