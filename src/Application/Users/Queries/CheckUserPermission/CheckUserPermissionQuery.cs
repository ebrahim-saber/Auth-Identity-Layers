using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IdentityAuthLayer.Application.Common.Interfaces.Services;
using IdentityAuthLayer.SharedModels.ResponseManager;

namespace IdentityAuthLayer.Application.Users.Queries.CheckUserPermission
{
    /// <summary>يستخدمها أي سيستم فرعي: اليوزر ده عنده الصلاحية دي على سيستمي؟</summary>
    public record CheckUserPermissionQuery(Guid UserId, Guid SystemId, string PermissionCode) : IRequest<Result<bool>>;

    public class CheckUserPermissionQueryHandler : IRequestHandler<CheckUserPermissionQuery, Result<bool>>
    {
        private readonly IUserService _userService;

        public CheckUserPermissionQueryHandler(IUserService userService) => _userService = userService;

        public Task<Result<bool>> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken) =>
            _userService.CheckUserPermissionAsync(request.UserId, request.SystemId, request.PermissionCode, cancellationToken);
    }
}
