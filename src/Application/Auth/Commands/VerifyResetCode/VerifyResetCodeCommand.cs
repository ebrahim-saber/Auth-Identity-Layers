using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.VerifyResetCode
{
    public record VerifyResetCodeCommand(string Email, string Code) : IRequest<Result>;

    public class VerifyResetCodeCommandValidator : AbstractValidator<VerifyResetCodeCommand>
    {
        public VerifyResetCodeCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Code).NotEmpty().Length(4, 8).Matches(@"^\d+$");
        }
    }

    public class VerifyResetCodeCommandHandler : IRequestHandler<VerifyResetCodeCommand, Result>
    {
        private readonly IAuthService _authService;

        public VerifyResetCodeCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(VerifyResetCodeCommand request, CancellationToken cancellationToken) =>
            _authService.VerifyResetCodeAsync(request.Email, request.Code, cancellationToken);
    }
}
