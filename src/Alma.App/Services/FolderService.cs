namespace Alma.Services;

public class FolderService : IFolderService
{
    public string? ConfigRoot { get; }
    public string AppData { get; }

    public FolderService()
    {
        ConfigRoot = GetConfigHomePath();
        AppData = GetAppDataPath();

        if (!Directory.Exists(AppData)) Directory.CreateDirectory(AppData);
    }

    private static string? GetConfigHomePath()
    {
        var configHomeProviders = new List<Func<string?>>
        {
            () => Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config")
        };

        var configHome = EnumerateProviders(configHomeProviders);
        return configHome == null ? null : Path.Combine(configHome, "alma");
    }

    private static string GetAppDataPath()
    {
        var appDataProviders = new List<Func<string?>>
        {
            () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        var appData = EnumerateProviders(appDataProviders) ?? Environment.CurrentDirectory;
        return Path.Combine(appData, "alma");
    }

    private static string? EnumerateProviders(List<Func<string?>> providers)
    {
        string? result = null;

        foreach (var provider in providers)
        {
            result = provider();
            if (result is not null && Directory.Exists(result)) break;
        }

        return result;
    }
}