using Microsoft.Extensions.Configuration;

namespace TestUmbraco.Services
{
    public interface ILoggingService
    {
        bool IsEnabled { get; }
        void LogInformation(string message);
        void LogInformation<T>(string message);
        void LogError(string message);
        void LogError<T>(string message);
        void LogError(string message, Exception exception);
        void LogError<T>(string message, Exception exception);
        void LogWarning(string message);
        void LogWarning<T>(string message);
    }
}