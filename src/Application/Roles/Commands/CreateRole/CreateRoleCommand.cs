using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Roles.Commands.CreateRole
{
    public record CreateRoleCommand(Guid SystemId, string Name, string? Description) : IRequest<Result<Guid>>;

    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.SystemId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
    {
        private readonly IRoleService _roleService;

        public CreateRoleCommandHandler(IRoleService roleService) => _roleService = roleService;

        public Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken) =>
            _roleService.CreateRoleAsync(request.SystemId, request.Name, request.Description, cancellationToken);
    }
}
