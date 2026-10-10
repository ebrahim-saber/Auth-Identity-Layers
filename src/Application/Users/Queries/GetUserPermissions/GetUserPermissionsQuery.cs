using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Users.Queries.GetUserPermissions
{
    /// <summary>يستخدمها أي سيستم فرعي: هات كل صلاحيات اليوزر ده على سيستمي أنا.</summary>
    public record GetUserPermissionsQuery(Guid UserId, Guid SystemId) : IRequest<Result<List<UserPermissionDto>>>;

    public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<List<UserPermissionDto>>>
    {
        private readonly IUserService _userService;

        public GetUserPermissionsQueryHandler(IUserService userService) => _userService = userService;

        public Task<Result<List<UserPermissionDto>>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken) =>
            _userService.GetUserPermissionsAsync(request.UserId, request.SystemId, cancellationToken);
    }
}
