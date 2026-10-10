using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Users.Commands.AssignRoleToUser
{
    public record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Result<Guid>>;

    public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
    {
        public AssignRoleToUserCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.RoleId).NotEmpty();
        }
    }

    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Result<Guid>>
    {
        private readonly IUserService _userService;

        public AssignRoleToUserCommandHandler(IUserService userService) => _userService = userService;

        public Task<Result<Guid>> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken) =>
            _userService.AssignRoleToUserAsync(request.UserId, request.RoleId, cancellationToken);
    }
}
