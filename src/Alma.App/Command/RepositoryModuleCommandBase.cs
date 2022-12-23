using Alma.Configuration.Repository;
using Alma.Helper;
using Alma.Services;

namespace Alma.Command;

public abstract class RepositoryModuleCommandBase : ICommand
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IPathHelperService _pathHelperService;
    public abstract string CommandString { get; }
    public abstract Task Run(List<string> parameters);

    protected RepositoryModuleCommandBase(
        IRepositoryConfiguration repositoryConfiguration,
        IPathHelperService pathHelperService)
    {
        _repositoryConfiguration = repositoryConfiguration;
        _pathHelperService = pathHelperService;
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
            fallbackSourceDirectory =
                repoConfig.RepositoryPath is { } repoPath
                    ? _pathHelperService.ResolvePath(repoPath)
                    : fallbackSourceDirectory;
            fallbackTargetDirectory =
                repoConfig.LinkPath is { } linkPath
                    ? _pathHelperService.ResolvePath(linkPath)
                    : fallbackTargetDirectory;
        }

        return (fallbackSourceDirectory, fallbackTargetDirectory);
    }
}