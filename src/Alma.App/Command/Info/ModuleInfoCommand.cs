using Alma.Configuration.Repository;
using Alma.Services;

namespace Alma.Command.Info;

public class ModuleInfoCommand : ICommand
{
    public string CommandString => "info";

    private readonly IFolderService _folderService;
    private readonly IRepositoryConfiguration _repositoryConfiguration;

    public ModuleInfoCommand(
        IFolderService folderService,
        IRepositoryConfiguration repositoryConfiguration
    )
    {
        _folderService = folderService;
        _repositoryConfiguration = repositoryConfiguration;
    }

    public Task Run(List<string> parameters)
    {
        //Add info REPO
        //Add info REPO MODULE
        Console.WriteLine("AppData folder: " + _folderService.AppData);

        if (_folderService.ConfigRoot is string configRoot)
        {
            Console.WriteLine("Configuration folder: " + configRoot);
        }
        else
        {
            Console.WriteLine("Configuration folder not exists.");
            Console.WriteLine("Preffered configuration folder is: " + Path.Combine(_folderService.GetPreferredConfigurationFolder(), _folderService.ApplicationSubfolderName));
        }

        Console.WriteLine();

        if (_repositoryConfiguration.Configuration.Repositories is var repositores && repositores?.Count > 0)
        {
            Console.WriteLine("Repositories:");
            foreach (var repository in repositores)
            {
                Console.Write(repository.Name);
                if (repository.RepositoryPath is not null && !Directory.Exists(repository.RepositoryPath))
                {
                    Console.Write($" (containing folder not exists {repository.RepositoryPath})");
                }
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("No repositories found");
        }

        return Task.CompletedTask;
    }
}