using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Systems.Commands.LinkClientToSystem
{
    public record LinkClientToSystemCommand(
        Guid ClientId,
        Guid SystemId,
        DateTime? ContractStartDate = null,
        DateTime? ContractEndDate = null,
        int? MaxUsersQuota = null) : IRequest<Result<Guid>>;

    public class LinkClientToSystemCommandValidator : AbstractValidator<LinkClientToSystemCommand>
    {
        public LinkClientToSystemCommandValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.SystemId).NotEmpty();
            RuleFor(x => x.MaxUsersQuota).GreaterThan(0).When(x => x.MaxUsersQuota.HasValue)
                .WithMessage("الحد الأقصى للمستخدمين يجب أن يكون أكبر من صفر");
        }
    }

    public class LinkClientToSystemCommandHandler : IRequestHandler<LinkClientToSystemCommand, Result<Guid>>
    {
        private readonly IClientService _clientService;

        public LinkClientToSystemCommandHandler(IClientService clientService) => _clientService = clientService;

        public Task<Result<Guid>> Handle(LinkClientToSystemCommand request, CancellationToken cancellationToken) =>
            _clientService.LinkClientToSystemAsync(
                request.ClientId,
                request.SystemId,
                request.ContractStartDate,
                request.ContractEndDate,
                request.MaxUsersQuota,
                cancellationToken);
    }
}
