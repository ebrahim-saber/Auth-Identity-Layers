namespace IdentityAuthLayer.SharedModels.ResponseManager
{
    /// <summary>نتيجة عملية بدون داتا (نجاح/فشل + رسالة) - بنفس فكرة الـ Result بتاعك.</summary>
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; } = string.Empty;
        public string? Error { get; protected set; }

        public static Result Success(string? message = null) =>
            new() { IsSuccess = true, Message = message ?? "تمت العملية بنجاح" };

        public static Result Failure(string error) =>
            new() { IsSuccess = false, Error = error };

        public static Result<T> Success<T>(T data, string? message = null) =>
            new() { IsSuccess = true, Data = data, Message = message ?? "تمت العملية بنجاح" };

        public static Result<T> Failure<T>(string error) =>
            new() { IsSuccess = false, Error = error };
    }

    /// <summary>نتيجة عملية بترجع بيانات من نوع T.</summary>
    public class Result<T> : Result
    {
        public T? Data { get; set; }
    }
}
