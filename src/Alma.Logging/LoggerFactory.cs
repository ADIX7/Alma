namespace Alma.Logging;

public class LoggerFactory : ILoggerFactory
{
    public LogLevel DefaultLogLevel { get; }

    public LoggerFactory(LogLevel defaultLogLevel = LogLevel.Information)
    {
        DefaultLogLevel = defaultLogLevel;
    }

    public ILogger<T> CreateLogger<T>()
    {
        return new Logger<T>(DefaultLogLevel);
    }
}