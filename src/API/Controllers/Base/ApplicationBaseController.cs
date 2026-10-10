using MediatR;
using Microsoft.AspNetCore.Mvc;
using IdentityAuthLayer.SharedModels.ResponseManager;
using IdentityAuthLayer.SharedModels.Responses;

namespace IdentityAuthLayer.API.Controllers.Base
{
    /// <summary>نفس فكرة ApplicationBaseController بتاعك - كل Controller بيورث منه.</summary>
    [ApiController]
    public class ApplicationBaseController : ControllerBase
    {
        protected IMediator _mediator => HttpContext.RequestServices.GetService<IMediator>()!;
        protected ResponseHandler _responseHandler => new();

        [NonAction]
        public ObjectResult BaseResponse(object? response)
        {
            if (response is null)
                return new BadRequestObjectResult(_responseHandler.BadRequest<object>("حدث خطأ غير متوقع"));

            // Result<T> -> ApplicationResponse<T>
            if (response.GetType().IsGenericType &&
                response.GetType().GetGenericTypeDefinition() == typeof(Result<>))
                return BaseResponse(_responseHandler.FromResult((dynamic)response));

            // Result -> ApplicationResponse<object>
            if (response is Result result)
                return BaseResponse(_responseHandler.FromResult(result));

            // ApplicationResponse<T> جاهز -> يترجم لـ StatusCode المناسب
            if (response.GetType().IsGenericType &&
                response.GetType().GetGenericTypeDefinition() == typeof(ApplicationResponse<>))
            {
                dynamic dynamicResponse = response;
                return dynamicResponse.StatusCode switch
                {
                    200 => new OkObjectResult(response),
                    201 => new CreatedResult(string.Empty, response),
                    400 => new BadRequestObjectResult(response),
                    401 => new UnauthorizedObjectResult(response),
                    404 => new NotFoundObjectResult(response),
                    _ => new BadRequestObjectResult(response)
                };
            }

            // أي حاجة تانية (Guid، List، bool، إلخ) بنلفها في ApplicationResponse برضو للثبات
            return new OkObjectResult(_responseHandler.Success(response));
        }
    }
}
