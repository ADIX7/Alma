using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Services;

namespace Alma.Command.List;

public class ListCommand : ICommand
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    public string CommandString => "ls";

    public ListCommand(
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver)
    {
        _repositoryConfiguration = repositoryConfiguration;
        _moduleConfigurationResolver = moduleConfigurationResolver;
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
        Console.WriteLine("Repositories:" + Environment.NewLine);
        foreach (var repository in _repositoryConfiguration.Configuration.Repositories)
        {
            Console.WriteLine(repository.Name);
        }

        return Task.CompletedTask;
    }

    private async Task ListModulesByRepoName(string repositoryName)
    {
        var repo = _repositoryConfiguration.Configuration.Repositories.FirstOrDefault(r => r.Name == repositoryName);
        if (repo is null)
        {
            Console.WriteLine($"No repository found with name '{repositoryName}'");
            return;
        }

        if (repo.RepositoryPath is null)
        {
            Console.WriteLine($"No repository path is specified in repository settings '{repositoryName}'");
            return;
        }

        await ListModules(repo.RepositoryPath, repositoryName);
    }

    private async Task ListModules(string repositoryPath, string repositoryName)
    {
        var repositoryDirectory = new DirectoryInfo(repositoryPath);
        var moduleDirectories = await TraverseRepositoryFolder(repositoryDirectory);

        Console.WriteLine($"Modules in repository '{repositoryName}':" + Environment.NewLine);
        foreach (var modulePath in moduleDirectories)
        {
            Console.WriteLine(modulePath.FullName.Substring(repositoryDirectory.FullName.Length).Replace(Path.DirectorySeparatorChar, '/'));
        }
    }

    private async Task<IEnumerable<DirectoryInfo>> TraverseRepositoryFolder(DirectoryInfo currentDirectory)
    {
        var moduleConfigFileStub = Path.Combine(currentDirectory.FullName, Constants.ModuleConfigFileStub);
        var (moduleConfiguration, moduleConfigurationFile) = await _moduleConfigurationResolver.ResolveModuleConfiguration(moduleConfigFileStub);

        var result = Enumerable.Empty<DirectoryInfo>();
        if (moduleConfigurationFile is not null)
        {
            result = new List<DirectoryInfo> {currentDirectory};
        }

        foreach (var subDir in currentDirectory.GetDirectories())
        {
            result = result.Concat(await TraverseRepositoryFolder(subDir));
        }

        return result;
    }
}