namespace Alma.Logging;

public interface ILoggerFactory
{
    ILogger<T> CreateLogger<T>();
    ILogger CreateLogger(Type t);
    LogLevel DefaultLogLevel { get; }
}