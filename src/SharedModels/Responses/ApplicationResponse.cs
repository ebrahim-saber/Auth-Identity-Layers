namespace IdentityAuthLayer.SharedModels.Responses
{
    /// <summary>شكل الرد الموحد للـ API (نفس فكرة CarCare.SharedModels.Responses).</summary>
    public class ApplicationResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }

    /// <summary>بيبني الـ ApplicationResponse ويحوّل الـ Result/Result&lt;T&gt; لشكل موحّد للـ Controller.</summary>
    public class ResponseHandler
    {
        public ApplicationResponse<T> Success<T>(T data, string? message = null) => new()
        {
            StatusCode = 200,
            Succeeded = true,
            Message = message ?? "تمت العملية بنجاح",
            Data = data
        };

        public ApplicationResponse<T> Created<T>(T data, string? message = null) => new()
        {
            StatusCode = 201,
            Succeeded = true,
            Message = message ?? "تم الإنشاء بنجاح",
            Data = data
        };

        public ApplicationResponse<T> BadRequest<T>(string message) => new()
        {
            StatusCode = 400,
            Succeeded = false,
            Message = message
        };

        public ApplicationResponse<T> Unauthorized<T>(string message = "غير مصرح") => new()
        {
            StatusCode = 401,
            Succeeded = false,
            Message = message
        };

        public ApplicationResponse<T> NotFound<T>(string message = "غير موجود") => new()
        {
            StatusCode = 404,
            Succeeded = false,
            Message = message
        };

        /// <summary>يحوّل Result (بدون داتا) لـ ApplicationResponse&lt;object&gt;.</summary>
        public ApplicationResponse<object> FromResult(IdentityAuthLayer.SharedModels.ResponseManager.Result result)
        {
            return result.IsSuccess
                ? Success<object>(null!, result.Message)
                : BadRequest<object>(result.Error ?? "حدث خطأ");
        }

        /// <summary>يحوّل Result&lt;T&gt; لـ ApplicationResponse&lt;T&gt;.</summary>
        public ApplicationResponse<T> FromResult<T>(IdentityAuthLayer.SharedModels.ResponseManager.Result<T> result)
        {
            return result.IsSuccess
                ? Success(result.Data!, result.Message)
                : BadRequest<T>(result.Error ?? "حدث خطأ");
        }
    }
}
