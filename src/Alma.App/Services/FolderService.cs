using System.Diagnostics.CodeAnalysis;
using Alma.Command.Diag;
using Alma.Logging;

namespace Alma.Services;

public class FolderService : IFolderService
{
    private readonly Dictionary<string, Func<string?>> _configHomeProviders;
    private readonly Dictionary<string, Func<string?>> _appDataProviders;
    public string? ConfigRoot { get; }
    public string AppData { get; }

    public string ApplicationSubfolderName => "alma";

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FolderService))]
    public FolderService()
    {
        _configHomeProviders = ConfigHomeProviders();
        _appDataProviders = AppDataProviders();

        ConfigRoot = GetConfigHomePath();
        AppData = GetAppDataPath();

        if (!Directory.Exists(AppData)) Directory.CreateDirectory(AppData);
    }

    private static Dictionary<string, Func<string?>> ConfigHomeProviders()
    {
        return new Dictionary<string, Func<string?>>
        {
            {"ALMA_CONFIG", () => Environment.GetEnvironmentVariable("ALMA_CONFIG")},
            {"XDG_CONFIG_HOME", () => Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")},
            {"UserProfile", GetPreferredConfigurationFolder},
            {"ALMA_CONFIG_FALLBACK", () => Environment.GetEnvironmentVariable("ALMA_CONFIG_FALLBACK")},
        };
    }

    private static Dictionary<string, Func<string?>> AppDataProviders()
    {
        return new Dictionary<string, Func<string?>>
        {
            {"ALMA_APP_DATA", () => Environment.GetEnvironmentVariable("ALMA_APP_DATA")},
            {"LocalApplicationData", () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)},
            {"ALMA_APP_DATA_FALLBACK", () => Environment.GetEnvironmentVariable("ALMA_APP_DATA_FALLBACK")},
        };
    }

    string IFolderService.GetPreferredConfigurationFolder()
        => GetPreferredConfigurationFolder();

    public static string GetPreferredConfigurationFolder()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

    private string? GetConfigHomePath()
    {
        var configHome = EnumerateProviders(_configHomeProviders.Values);
        return configHome == null ? null : Path.Combine(configHome, ApplicationSubfolderName);
    }

    private string GetAppDataPath()
    {
        var appData = EnumerateProviders(_appDataProviders.Values) ?? Environment.CurrentDirectory;
        return Path.Combine(appData, ApplicationSubfolderName);
    }

    private static string? EnumerateProviders(IEnumerable<Func<string?>> providers)
    {
        string? result = null;

        foreach (var provider in providers)
        {
            result = provider();
            if (result is not null && Directory.Exists(result)) break;
        }

        return result;
    }

    [DiagnosticHelper("config-home-providers")]
    public static void ConfigHomeProviderDiag(ILogger logger)
    {
        var configHomeProviders = ConfigHomeProviders();
        logger.LogInformation($"There are {configHomeProviders.Count} config home providers:");
        foreach (var configHome in configHomeProviders)
        {
            logger.LogInformation($"{configHome.Key} => {configHome.Value() ?? "<null>"}");
        }
    }

    [DiagnosticHelper("app-data-providers")]
    public static void AppDataProviderDiag(ILogger logger)
    {
        var appDataProviders = AppDataProviders();
        logger.LogInformation($"There are {appDataProviders.Count} app data providers:");
        foreach (var appData in appDataProviders)
        {
            logger.LogInformation($"{appData.Key} => {appData.Value() ?? "<null>"}");
        }
    }
}