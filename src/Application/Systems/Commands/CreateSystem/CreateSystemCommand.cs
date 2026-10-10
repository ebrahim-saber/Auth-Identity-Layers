using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Systems.Commands.CreateSystem
{
    public record CreateSystemCommand(string Code, string Name, string? Description) : IRequest<Result<Guid>>;

    public class CreateSystemCommandValidator : AbstractValidator<CreateSystemCommand>
    {
        public CreateSystemCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public class CreateSystemCommandHandler : IRequestHandler<CreateSystemCommand, Result<Guid>>
    {
        private readonly ISystemService _systemService;

        public CreateSystemCommandHandler(ISystemService systemService) => _systemService = systemService;

        public Task<Result<Guid>> Handle(CreateSystemCommand request, CancellationToken cancellationToken) =>
            _systemService.CreateSystemAsync(request.Code, request.Name, request.Description, cancellationToken);
    }
}
