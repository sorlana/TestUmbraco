using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace TestUmbraco.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly IConfiguration _configuration;
        
        public bool IsEnabled => _configuration.GetValue<bool>("Logging:Enabled", true);

        public LoggingService(
            IConfiguration configuration,
            ILogger<LoggingService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            if (!IsEnabled) return;
            _logger.LogInformation(message);
            WriteColored(message, ConsoleColor.Green);
        }

        public void LogInformation<T>(string message)
        {
            if (!IsEnabled) return;
            var fullMessage = $"[{typeof(T).Name}] {message}";
            _logger.LogInformation(fullMessage);
            WriteColored(fullMessage, ConsoleColor.Green);
        }

        public void LogError(string message)
        {
            if (!IsEnabled) return;
            _logger.LogError(message);
            WriteColored(message, ConsoleColor.Red);
        }

        public void LogError<T>(string message)
        {
            if (!IsEnabled) return;
            var fullMessage = $"[{typeof(T).Name}] {message}";
            _logger.LogError(fullMessage);
            WriteColored(fullMessage, ConsoleColor.Red);
        }

        public void LogError(string message, Exception exception)
        {
            if (!IsEnabled) return;
            _logger.LogError(exception, message);
            WriteColored($"{message}: {exception.Message}", ConsoleColor.DarkRed);
        }

        public void LogError<T>(string message, Exception exception)
        {
            if (!IsEnabled) return;
            var fullMessage = $"[{typeof(T).Name}] {message}";
            _logger.LogError(exception, fullMessage);
            WriteColored($"{fullMessage}: {exception.Message}", ConsoleColor.DarkRed);
        }

        public void LogWarning(string message)
        {
            if (!IsEnabled) return;
            _logger.LogWarning(message);
            WriteColored(message, ConsoleColor.Yellow);
        }

        public void LogWarning<T>(string message)
        {
            if (!IsEnabled) return;
            var fullMessage = $"[{typeof(T).Name}] {message}";
            _logger.LogWarning(fullMessage);
            WriteColored(fullMessage, ConsoleColor.Yellow);
        }

        private void WriteColored(string message, ConsoleColor color)
        {
            try
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} {message}");
                Console.ForegroundColor = originalColor;
            }
            catch
            {
                // Игнорируем ошибки консоли
            }
        }
    }
}