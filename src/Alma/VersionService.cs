using System.Reflection;
using Alma.Services;

namespace Alma;

public class VersionService : IVersionService
{
    public string GetVersion()
    {
        return
            typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(Program).Assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}