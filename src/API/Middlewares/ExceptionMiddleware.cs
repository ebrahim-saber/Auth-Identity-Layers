using System.Net;
using System.Text.Json;
using FluentValidation;
using IdentityAuthLayer.SharedModels.Responses;

namespace IdentityAuthLayer.API.Middlewares
{
    /// <summary>
    /// بيلقط أي InvalidOperationException / UnauthorizedAccessException طالعة من الـ Handlers
    /// (قواعد زي "الرول والبرميشن لازم يكونوا على نفس السيستم") ويرجعها بشكل ApplicationResponse موحّد.
    /// وكمان بيلقط FluentValidation.ValidationException الطالعة من ValidationBehavior (MediatR pipeline)
    /// ويرجع كل رسائل الأخطاء في Errors.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage).Distinct().ToList();
                await WriteResponse(context, HttpStatusCode.BadRequest, "خطأ في البيانات المدخلة", errors);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteResponse(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                await WriteResponse(context, HttpStatusCode.InternalServerError, "حدث خطأ غير متوقع في السيرفر: " + ex.Message);
            }
        }

        private static Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message, List<string>? errors = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ApplicationResponse<object>
            {
                StatusCode = (int)statusCode,
                Succeeded = false,
                Message = message,
                Errors = errors
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
