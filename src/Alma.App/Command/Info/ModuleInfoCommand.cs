using Alma.Configuration.Repository;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Info;

public class ModuleInfoCommand : ICommand
{
    public string CommandString => "info";

    private readonly IFolderService _folderService;
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly ILogger<ModuleInfoCommand> _logger;

    public ModuleInfoCommand(
        IFolderService folderService,
        IRepositoryConfiguration repositoryConfiguration,
        ILogger<ModuleInfoCommand> logger
    )
    {
        _folderService = folderService;
        _repositoryConfiguration = repositoryConfiguration;
        _logger = logger;
    }

    public Task Run(List<string> parameters)
    {
        //Add info REPO
        //Add info REPO MODULE
        _logger.LogInformation("AppData folder: " + _folderService.AppData);

        if (_folderService.ConfigRoot is string configRoot)
        {
            _logger.LogInformation("Configuration folder: " + configRoot);
        }
        else
        {
            _logger.LogInformation("Configuration folder not exists.");
            _logger.LogInformation("Preffered configuration folder is: " + Path.Combine(_folderService.GetPreferredConfigurationFolder(), _folderService.ApplicationSubfolderName));
        }

        _logger.LogInformation("");

        if (_repositoryConfiguration.Configuration.Repositories is var repositores && repositores?.Count > 0)
        {
            _logger.LogInformation("Repositories:");
            foreach (var repository in repositores)
            {
                Console.Write(repository.Name);
                if (repository.RepositoryPath is not null && !Directory.Exists(repository.RepositoryPath))
                {
                    Console.Write($" (containing folder not exists {repository.RepositoryPath})");
                }
                _logger.LogInformation("");
            }
        }
        else
        {
            _logger.LogInformation("No repositories found");
        }

        return Task.CompletedTask;
    }
}