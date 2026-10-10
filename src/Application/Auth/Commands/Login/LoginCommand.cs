using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.Login
{
    public record LoginCommand(string Username, string Password) : IRequest<Result<AuthTokensDto>>;

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken cancellationToken) =>
            _authService.LoginAsync(request.Username, request.Password, cancellationToken);
    }
}
