using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.VerifyAccount
{
    public record VerifyAccountCommand(string Email, string VerificationCode) : IRequest<Result>;

    public class VerifyAccountCommandValidator : AbstractValidator<VerifyAccountCommand>
    {
        public VerifyAccountCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.VerificationCode).NotEmpty().Length(6).Matches(@"^\d{6}$");
        }
    }

    public class VerifyAccountCommandHandler : IRequestHandler<VerifyAccountCommand, Result>
    {
        private readonly IAuthService _authService;

        public VerifyAccountCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(VerifyAccountCommand request, CancellationToken cancellationToken) =>
            _authService.VerifyAccountAsync(request.Email, request.VerificationCode, cancellationToken);
    }
}
