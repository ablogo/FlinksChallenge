using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class LogService<T> : ILog
    {
        private readonly ILogger _log;

        public LogService(ILoggerFactory loggerFactory, string categoryName)
        {
            _log = loggerFactory.CreateLogger(categoryName);
        }

        public LogService(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<T>();
        }

        public void Debug(string message)
        {
            _log.LogDebug(message);
        }

        public void Error(string message)
        {
            _log.LogError(message);
        }

        public void Information(string message)
        {
            _log.LogInformation(message);
        }

        public void Warning(string message)
        {
            _log.LogWarning(message);
        }

    }
}
