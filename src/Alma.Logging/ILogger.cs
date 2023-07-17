namespace Alma.Logging;

public interface ILogger
{
    LogLevel DefaultLogLevel { get; }
    void LogInformation(string logMessage);
    void LogDebug(string logMessage);
    void LogTrace(string logMessage);
    void Log(string logMessage, LogLevel logLevel);
    void LogError(string logMessage);
    void LogCritical(string logMessage);
}

public interface ILogger<T> : ILogger
{
    
}