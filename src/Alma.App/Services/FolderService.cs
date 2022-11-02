namespace Alma.Services;

public class FolderService : IFolderService
{
    public string? ConfigRoot { get; }
    public string AppData { get; }

    public string ApplicationSubfolderName => "alma";

    public FolderService()
    {
        ConfigRoot = GetConfigHomePath();
        AppData = GetAppDataPath();

        if (!Directory.Exists(AppData)) Directory.CreateDirectory(AppData);
    }

    public string GetPreferredConfigurationFolder()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

    private string? GetConfigHomePath()
    {
        var configHomeProviders = new List<Func<string?>>
        {
            () => Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            () => GetPreferredConfigurationFolder()
        };

        var configHome = EnumerateProviders(configHomeProviders);
        return configHome == null ? null : Path.Combine(configHome, ApplicationSubfolderName);
    }

    private string GetAppDataPath()
    {
        var appDataProviders = new List<Func<string?>>
        {
            () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        var appData = EnumerateProviders(appDataProviders) ?? Environment.CurrentDirectory;
        return Path.Combine(appData, ApplicationSubfolderName);
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