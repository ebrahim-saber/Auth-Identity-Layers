using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Roles.Commands.AssignPermissionToRole
{
    public record AssignPermissionToRoleCommand(Guid RoleId, Guid PermissionId) : IRequest<Result<Guid>>;

    public class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
    {
        public AssignPermissionToRoleCommandValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
            RuleFor(x => x.PermissionId).NotEmpty();
        }
    }

    public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, Result<Guid>>
    {
        private readonly IRoleService _roleService;

        public AssignPermissionToRoleCommandHandler(IRoleService roleService) => _roleService = roleService;

        public Task<Result<Guid>> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken) =>
            _roleService.AssignPermissionToRoleAsync(request.RoleId, request.PermissionId, cancellationToken);
    }
}
