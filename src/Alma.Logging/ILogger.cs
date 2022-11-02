namespace Alma.Logging;

public interface ILogger<T>
{
    LogLevel DefaultLogLevel { get; }
    void LogInformation(string logMessage);
    void LogDebug(string logMessage);
    void LogTrace(string logMessage);
    void Log(string logMessage, LogLevel logLevel);
}