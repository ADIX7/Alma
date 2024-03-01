using Alma.Command;
using Alma.Command.Configure;
using Alma.Command.Diag;
using Alma.Command.Help;
using Alma.Command.Info;
using Alma.Command.Install;
using Alma.Command.Link;
using Alma.Command.List;
using Alma.Command.Unlink;
using Alma.Configuration.Repository;
using Alma.Logging;
using Alma.Services;
using Jab;

namespace Alma;

public static class Program
{
    public static async Task Main(string[] args)
    {
        InitLogging();

        var logger = AlmaLoggerFactory.CreateLogger(typeof(Program));

        var workdir = GetWorkdir(logger);
        if (workdir != null)
        {
            Environment.CurrentDirectory = workdir;
        }

        var services = new AlmaServiceProvider();

        var repositoryConfiguration = services.GetService<IRepositoryConfiguration>();
        await repositoryConfiguration.LoadAsync();
        var application = services.GetService<Application>();

        await application.Run(args);
    }

    private static string? GetWorkdir(ILogger logger)
    {
        var workdirProviders = new Dictionary<string, Func<string?>>
        {
            {"ALMA_WORKDIR", () => Environment.GetEnvironmentVariable("ALMA_WORKDIR")},
            {"WORKDIR", () => Environment.GetEnvironmentVariable("WORKDIR")},
        };

        foreach (var workdirProvider in workdirProviders)
        {
            var workdir = workdirProvider.Value();
            if (workdir != null)
            {
                if (Directory.Exists(workdir))
                {
                    return workdir;
                }
                else
                {
                    logger.LogInformation($"{workdirProvider.Key} is set to {workdir} but this directory does not exist.");
                }
            }
        }

        return null;
    }

    private static void InitLogging()
    {
        AlmaLoggerFactory = new LoggerFactory();
    }

    public static ILoggerFactory AlmaLoggerFactory { get; private set; } = null!;
}

[ServiceProvider]
[Singleton(typeof(IRepositoryConfiguration), typeof(RepositoryConfiguration))]
[Singleton(typeof(IFolderService), typeof(FolderService))]
[Singleton(typeof(ConfigurationFileReader))]
[Singleton(typeof(IConfigurationFileReader), typeof(JsonConfigurationFileReader))]
[Singleton(typeof(IOsInformation), typeof(OsInformation))]
[Singleton(typeof(ICommand), typeof(LinkCommand))]
[Singleton(typeof(ICommand), typeof(UnlinkCommand))]
[Singleton(typeof(ICommand), typeof(InfoCommand))]
[Singleton(typeof(ICommand), typeof(ListCommand))]
[Singleton(typeof(ICommand), typeof(InstallCommand))]
[Singleton(typeof(ICommand), typeof(HelpCommand))]
[Singleton(typeof(ICommand), typeof(ConfigureCommand))]
[Singleton(typeof(ICommand), typeof(DiagCommand))]
[Singleton(typeof(IModuleConfigurationResolver), typeof(ModuleConfigurationResolver))]
[Singleton(typeof(IMetadataHandler), typeof(MetadataHandler))]
[Singleton(typeof(IShellService), typeof(ShellService))]
[Singleton(typeof(IVersionService), typeof(VersionService))]
[Singleton(typeof(IPathHelperService), typeof(PathHelperService))]
[Singleton(typeof(Application))]
[Transient(typeof(ILogger<>), Factory = nameof(CustomLoggerFactory))]
internal partial class AlmaServiceProvider
{
    public ILogger<T> CustomLoggerFactory<T>()
    {
        return Program.AlmaLoggerFactory.CreateLogger<T>();
    }
}