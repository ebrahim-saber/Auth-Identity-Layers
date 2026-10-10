using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Clients.Commands.CreateClient
{
    public record CreateClientCommand(string Name, string? ContactEmail, string? ContactPhone)
        : IRequest<Result<Guid>>;

    public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
    {
        public CreateClientCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        }
    }

    /// <summary>الهاندلر رفيع - بس بينادي السيرفيس وبيرجّع اللي هو راجعه (Result&lt;Guid&gt;).</summary>
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<Guid>>
    {
        private readonly IClientService _clientService;

        public CreateClientCommandHandler(IClientService clientService) => _clientService = clientService;

        public Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken) =>
            _clientService.CreateClientAsync(request.Name, request.ContactEmail, request.ContactPhone, cancellationToken);
    }
}
