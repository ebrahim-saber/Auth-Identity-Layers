using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.Domain.Enums;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Users.Commands.CreateUser
{
    /// <summary>إضافة يوزر مباشرة بمعرفة الأدمن (بكلمة سر جاهزة) - مختلف عن Register (اللي فيه verification flow).</summary>
    public record CreateUserCommand(
        Guid ClientId,
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        string Password,
        UserType UserType) : IRequest<Result<Guid>>;

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty().Matches(@"^01[0-9]{9}$").WithMessage("رقم التليفون غير صحيح");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
    {
        private readonly IUserService _userService;

        public CreateUserCommandHandler(IUserService userService) => _userService = userService;

        public Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken) =>
            _userService.CreateUserAsync(request.ClientId, request.Username, request.FirstName, request.LastName,
                request.Email, request.Phone, request.Password, request.UserType, cancellationToken);
    }
}
