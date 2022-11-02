using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.List;

public class ListCommand : ICommand
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly ILogger<ListCommand> _logger;

    public string CommandString => "ls";

    public ListCommand(
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        ILogger<ListCommand> logger)
    {
        _repositoryConfiguration = repositoryConfiguration;
        _moduleConfigurationResolver = moduleConfigurationResolver;
        _logger = logger;
    }

    public async Task Run(List<string> parameters)
    {
        if (parameters.Count > 0)
        {
            await ListModulesByRepoName(parameters[0]);
        }
        else
        {
            await ListRepositories();
        }
    }

    private Task ListRepositories()
    {
        _logger.LogInformation("Repositories");
        foreach (var repository in _repositoryConfiguration.Configuration.Repositories)
        {
            _logger.LogInformation(repository.Name);
        }

        return Task.CompletedTask;
    }

    private async Task ListModulesByRepoName(string repositoryName)
    {
        var repo = _repositoryConfiguration.Configuration.Repositories.FirstOrDefault(r => r.Name == repositoryName);
        if (repo is null)
        {
            _logger.LogInformation($"No repository found with name '{repositoryName}'");
            return;
        }

        if (repo.RepositoryPath is null)
        {
            _logger.LogInformation($"No repository path is specified in repository settings '{repositoryName}'");
            return;
        }

        await ListModules(repo.RepositoryPath, repositoryName);
    }

    private async Task ListModules(string repositoryPath, string repositoryName)
    {
        var repositoryDirectory = new DirectoryInfo(repositoryPath);
        var moduleDirectories = await TraverseRepositoryFolder(repositoryDirectory);

        _logger.LogInformation($"Modules in repository '{repositoryName}'");
        foreach (var modulePath in moduleDirectories)
        {
            _logger.LogInformation(modulePath.FullName[repositoryDirectory.FullName.Length..].TrimStart(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/'));
        }
    }

    private async Task<IEnumerable<DirectoryInfo>> TraverseRepositoryFolder(DirectoryInfo currentDirectory)
    {
        var moduleConfigFileStub = Path.Combine(currentDirectory.FullName, Constants.ModuleConfigFileStub);
        var (_, moduleConfigurationFile) = await _moduleConfigurationResolver.ResolveModuleConfiguration(moduleConfigFileStub);

        var result = Enumerable.Empty<DirectoryInfo>();
        if (moduleConfigurationFile is not null)
        {
            result = new List<DirectoryInfo> { currentDirectory };
        }

        foreach (var subDir in currentDirectory.GetDirectories())
        {
            result = result.Concat(await TraverseRepositoryFolder(subDir));
        }

        return result;
    }
}