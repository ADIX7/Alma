using Alma.Configuration.Repository;
using Alma.Helper;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Info;

public class InfoCommand : ICommand
{
    public string CommandString => "info";

    private readonly IFolderService _folderService;
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly ILogger<InfoCommand> _logger;
    private readonly IOsInformation _osInformation;

    public InfoCommand(
        IFolderService folderService,
        IRepositoryConfiguration repositoryConfiguration,
        ILogger<InfoCommand> logger,
        IOsInformation osInformation
    )
    {
        _folderService = folderService;
        _repositoryConfiguration = repositoryConfiguration;
        _logger = logger;
        _osInformation = osInformation;
    }

    public async Task Run(List<string> parameters)
    {
        //Add info REPO
        //Add info REPO MODULE
        _logger.LogInformation("AppData folder: " + _folderService.AppData);

        if (_folderService.ConfigRoot is { } configRoot)
        {
            _logger.LogInformation("Configuration folder: " + configRoot);
        }
        else
        {
            _logger.LogInformation("Configuration folder not exists.");
            _logger.LogInformation("Preferred configuration folder is: " + Path.Combine(_folderService.GetPreferredConfigurationFolder(), _folderService.ApplicationSubfolderName));
        }

        _logger.LogInformation("");
        _logger.LogInformation($"Platform is '{await _osInformation.GetOsIdentifierAsync()}'");
        _logger.LogInformation("");

        if (_repositoryConfiguration.Configuration.Repositories is {Count: > 0} repositories)
        {
            _logger.LogInformation("Repositories:");
            foreach (var repository in repositories)
            {
                Console.Write(repository.Name);
                if (repository.RepositoryPath is not null && !Directory.Exists(PathHelper.ResolvePath(repository.RepositoryPath)))
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
    }
}