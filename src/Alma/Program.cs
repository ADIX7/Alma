using Alma.Command;
using Alma.Command.Help;
using Alma.Command.Info;
using Alma.Command.Link;
using Alma.Command.List;
using Alma.Command.Unlink;
using Alma.Configuration.Repository;
using Alma.Services;
using Jab;

namespace Alma;

public static class Program
{
    /*public static async Task Main(string[] args)
    {
        var services = BuildServices();

        var repositoryConfiguration = services.GetRequiredService<IRepositoryConfiguration>();
        await repositoryConfiguration.LoadAsync();
        var application = services.GetRequiredService<Application>();

        await application.Run(args);

        static IServiceProvider BuildServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IRepositoryConfiguration, RepositoryConfiguration>();
            serviceCollection.AddSingleton<IFolderService, FolderService>();
            serviceCollection.AddSingleton<ConfigurationFileReader>();
            serviceCollection.AddSingleton<IConfigurationFileReader, JsonConfigurationFileReader>();
            serviceCollection.AddSingleton<IOsInformation, OsInformation>();
            serviceCollection.AddSingleton<ICommand, LinkCommand>();
            serviceCollection.AddSingleton<IModuleConfigurationResolver, ModuleConfigurationResolver>();
            serviceCollection.AddSingleton<Application>();

            typeof(IRepositoryConfiguration), typeof(RepositoryConfiguration)
            typeof(IFolderService), typeof(FolderService)
            typeof(ConfigurationFileReader)
            typeof(IConfigurationFileReader), typeof(JsonConfigurationFileReader)
            typeof(IOsInformation), typeof(OsInformation)
            typeof(ICommand), typeof(LinkCommand)
            typeof(IModuleConfigurationResolver), typeof(ModuleConfigurationResolver)
            typeof(Application)

            return serviceCollection.BuildServiceProvider();
        }
    }*/

    public static async Task Main(string[] args)
    {
        var services = new AlmaServiceProvider();

        var repositoryConfiguration = services.GetService<IRepositoryConfiguration>();
        await repositoryConfiguration.LoadAsync();
        var application = services.GetService<Application>();

        await application.Run(args);
    }
}

[ServiceProvider]
[Singleton(typeof(IRepositoryConfiguration), typeof(RepositoryConfiguration))]
[Singleton(typeof(IFolderService), typeof(FolderService))]
[Singleton(typeof(ConfigurationFileReader))]
[Singleton(typeof(IConfigurationFileReader), typeof(JsonConfigurationFileReader))]
[Singleton(typeof(IOsInformation), typeof(OsInformation))]
[Singleton(typeof(ICommand), typeof(LinkCommand))]
[Singleton(typeof(ICommand), typeof(UnlinkCommand))]
[Singleton(typeof(ICommand), typeof(ModuleInfoCommand))]
[Singleton(typeof(ICommand), typeof(ListCommand))]
//Dependency cycle
//[Singleton(typeof(ICommand), typeof(HelpCommand))]
[Singleton(typeof(IModuleConfigurationResolver), typeof(ModuleConfigurationResolver))]
[Singleton(typeof(IMetadataHandler), typeof(MetadataHandler))]
[Singleton(typeof(Application))]
internal partial class AlmaServiceProvider
{

}