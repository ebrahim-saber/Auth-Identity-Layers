using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Auth.Commands.Register
{
    /// <summary>تسجيل حساب جديد - بيبعت كود تفعيل على الإيميل (الباسورد بتتحط بعدين في CreatePassword).</summary>
    public record RegisterCommand(Guid ClientId, string Username, string FirstName, string LastName, string Email, string Phone)
        : IRequest<Result>;

    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
            RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2).MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MinimumLength(2).MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().Matches(new Regex(@"^01[0-9]{9}$")).WithMessage("رقم التليفون غير صحيح");
        }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly IAuthService _authService;

        public RegisterCommandHandler(IAuthService authService) => _authService = authService;

        public Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken) =>
            _authService.RegisterAsync(request.ClientId, request.Username, request.FirstName, request.LastName,
                request.Email, request.Phone, cancellationToken);
    }
}
