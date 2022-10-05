using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

namespace NetDocsFileQueueFlusher.FileLogging
{
    public class FileLogger : ILogger
    {
        protected readonly FileLoggerProvider _loggerProvider;

        public FileLogger([NotNull] FileLoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
        }


        public IDisposable? BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var _fullFilePath = $"{_loggerProvider._options.FolderPath}/{_loggerProvider._options.FilePath.Replace("{date}", DateTime.UtcNow.ToString("dd-MM-yyyy"))}";
            var _logRecord = string.Format("{0} [{1}] {2} {3}", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"), logLevel.ToString(), formatter(state, exception), exception != null ? exception.StackTrace : "");

            using (var _sw = new StreamWriter(_fullFilePath, true))
            {
                _sw.WriteLine(_logRecord);
            }
        }
    }
}
