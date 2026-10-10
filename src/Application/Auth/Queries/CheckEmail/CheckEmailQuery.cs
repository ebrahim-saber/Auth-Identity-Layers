using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Queries.CheckEmail
{
    public record CheckEmailQuery(string Email) : IRequest<Result<EmailCheckResultDto>>;

    public class CheckEmailQueryValidator : AbstractValidator<CheckEmailQuery>
    {
        public CheckEmailQueryValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class CheckEmailQueryHandler : IRequestHandler<CheckEmailQuery, Result<EmailCheckResultDto>>
    {
        private readonly IAuthService _authService;

        public CheckEmailQueryHandler(IAuthService authService) => _authService = authService;

        public Task<Result<EmailCheckResultDto>> Handle(CheckEmailQuery request, CancellationToken cancellationToken) =>
            _authService.CheckEmailAsync(request.Email, cancellationToken);
    }
}
