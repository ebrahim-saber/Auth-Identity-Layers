using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.Logout
{
    public record LogoutCommand(Guid UserId) : IRequest<Result>;
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IAuthService _authService;
        public LogoutCommandHandler(IAuthService authService) => _authService = authService;
        public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken) => _authService.LogoutAsync(request.UserId, cancellationToken);
    }
}
