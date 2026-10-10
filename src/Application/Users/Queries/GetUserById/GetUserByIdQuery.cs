using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Users.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IUserService _userService;

        public GetUserByIdQueryHandler(IUserService userService) => _userService = userService;

        public Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken) =>
            _userService.GetUserByIdAsync(request.UserId, cancellationToken);
    }
}
