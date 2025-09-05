using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace SimpleFileBrowser.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;
        private readonly object _lock = new object();

        public FileLogger(string filePath, string categoryName)
        {
            _filePath = filePath;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logEntry = $"[{timestamp}] [{logLevel}] [{_categoryName}] {message}";

            if (exception != null)
            {
                logEntry += $"{Environment.NewLine}Exception: {exception}";
            }

            logEntry += Environment.NewLine;

            lock (_lock)
            {
                File.AppendAllText(_filePath, logEntry);
            }
        }
    }
}
