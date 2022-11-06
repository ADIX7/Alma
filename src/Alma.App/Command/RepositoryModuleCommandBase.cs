using Alma.Configuration.Repository;

namespace Alma.Command;

public abstract class RepositoryModuleCommandBase : ICommand
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    public abstract string CommandString { get; }
    public abstract Task Run(List<string> parameters);

    protected RepositoryModuleCommandBase(IRepositoryConfiguration repositoryConfiguration)
    {
        _repositoryConfiguration = repositoryConfiguration;
    }
    
    protected (string?, string?) GetRepositoryAndModuleName(List<string> parameters)
    {
        //TODO: handle parameters
        string? repositoryName = null;
        string? moduleName = null;

        if (parameters.Count == 1)
        {
            moduleName = parameters[0];
        }
        else if (parameters.Count >= 1)
        {
            repositoryName = parameters[0];
            moduleName = parameters[1];
        }

        return (repositoryName, moduleName);
    }

    protected (string sourceDirectory, string targetDirectory) GetModuleSourceAndTargetDirectory(string? repoName, string fallbackSourceDirectory, string fallbackTargetDirectory)
    {
        if (repoName is not null
            && _repositoryConfiguration.Configuration.Repositories.FirstOrDefault(r => r.Name == repoName) is { } repoConfig)
        {
            fallbackSourceDirectory = repoConfig.RepositoryPath ?? fallbackSourceDirectory;
            fallbackTargetDirectory = repoConfig.LinkPath ?? fallbackTargetDirectory;
        }

        return (fallbackSourceDirectory, fallbackTargetDirectory);
    }
}