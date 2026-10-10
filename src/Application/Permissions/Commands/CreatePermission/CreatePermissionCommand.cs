using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Permissions.Commands.CreatePermission
{
    public record CreatePermissionCommand(Guid SystemId, string Code, string Name, string? Description) : IRequest<Result<Guid>>;

    public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
    {
        public CreatePermissionCommandValidator()
        {
            RuleFor(x => x.SystemId).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        }
    }

    public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<Guid>>
    {
        private readonly IPermissionService _permissionService;

        public CreatePermissionCommandHandler(IPermissionService permissionService) => _permissionService = permissionService;

        public Task<Result<Guid>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken) =>
            _permissionService.CreatePermissionAsync(request.SystemId, request.Code, request.Name, request.Description, cancellationToken);
    }
}
