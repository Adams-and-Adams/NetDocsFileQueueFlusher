namespace NetDocsFileQueueFlusher.FileLogging
{
    public static class FileLoggerExtentions
    {
        public static ILoggingBuilder AddFileLogger (this ILoggingBuilder _loggingBuilder, Action<FileLoggerOptions> _configure)
        {
            _loggingBuilder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            _loggingBuilder.Services.Configure(_configure);

            return _loggingBuilder;
        }
    }
}
