namespace Alma.Logging;

public class Logger<T> : ILogger<T>
{
    public LogLevel DefaultLogLevel { get; }

    public Logger(LogLevel defaultLogLevel)
    {
        DefaultLogLevel = defaultLogLevel;
    }

    public void LogInformation(string s) => Log(s, LogLevel.Information);

    public void LogDebug(string s) => Log(s, LogLevel.Debug);

    public void LogTrace(string s) => Log(s, LogLevel.Trace);

    public void Log(string s, LogLevel logLevel)
    {
        if (logLevel <= DefaultLogLevel)
        {
            Console.WriteLine(s);
        }
    }

    public void LogError(string logMessage)
    {
        Log(logMessage, LogLevel.Error);
    }

    public void LogCritical(string logMessage)
    {
        Log(logMessage, LogLevel.Critical);
    }
}