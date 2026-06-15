namespace DemoApp.Web.Services.Utils
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public bool IsWarning { get; protected set; }
        public string? ErrorMessage { get; protected set; }

        public static Result Success() => new() { IsSuccess = true };

        public static Result Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };

        public static Result Warning(string message) => new() { IsSuccess = false, IsWarning = true, ErrorMessage = message };
    }

    public class Result<T> : Result
    {
        public T? Data { get; protected set; }

        public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };

        public static new Result<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };

        public static Result<T> Warning(string message, T? data = default) =>
            new() { IsSuccess = false, IsWarning = true, ErrorMessage = message, Data = data };

        public static implicit operator bool(Result<T> result) => result.IsSuccess;
    }
}

