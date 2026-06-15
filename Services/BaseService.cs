using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;

namespace DemoApp.Web.Services
{
    public abstract class BaseService<T>
    {
        private readonly string _serviceName;

        protected readonly ILogger<BaseService<T>> _logger;
        protected readonly ICurrentUserService _currentUserService;

        protected BaseService(ILogger<BaseService<T>> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _serviceName = typeof(T).Name;
        }

        protected string GetUsername() => _currentUserService.GetUsername();

        private string FormatMessage(string level, string message)
        {
            var user = GetUsername();
            return $"[{level}] [{_serviceName}] [User: {user}] {message}";
        }

        protected void LogInfo(string message)
        {
            _logger.LogInformation(FormatMessage("INFO", message));
        }

        protected void LogWarning(string message)
        {
            _logger.LogWarning(FormatMessage("WARN", message));
        }

        protected void LogError(string message, Exception? ex = null)
        {
            var formattedMessage = FormatMessage("ERROR", message);
            if (ex != null)
                _logger.LogError(ex, formattedMessage);
            else
                _logger.LogError(formattedMessage);
        }

        protected async Task<Result> ExecuteAsync(Func<Task> action, string logMessage, string userMessage)
        {
            try
            {
                await action();
                return Result.Success();
            }
            catch (Exception ex)
            {
                LogError(logMessage, ex);
                return Result.Failure(userMessage);
            }
        }

        protected async Task<Result<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> action, string logMessage, string userMessage)
        {
            try
            {
                var result = await action();
                return Result<TResult>.Success(result);
            }
            catch (Exception ex)
            {
                LogError(logMessage, ex);
                return Result<TResult>.Failure(userMessage);
            }
        }
    }
}
