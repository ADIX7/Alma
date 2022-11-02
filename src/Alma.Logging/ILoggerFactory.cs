namespace Alma.Logging;

public interface ILoggerFactory
{
    ILogger<T> CreateLogger<T>();
    LogLevel DefaultLogLevel { get; }
}