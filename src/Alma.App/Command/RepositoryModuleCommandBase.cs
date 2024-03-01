using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Services;

namespace Alma.Command;

public abstract class RepositoryModuleCommandBase : ICommand
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly IPathHelperService _pathHelperService;
    public abstract string CommandString { get; }
    public abstract string[] CommandAliases { get; }

    public abstract Task Run(List<string> parameters);

    protected RepositoryModuleCommandBase(
        IRepositoryConfiguration repositoryConfiguration,
        IPathHelperService pathHelperService,
        IModuleConfigurationResolver moduleConfigurationResolver)
    {
        _repositoryConfiguration = repositoryConfiguration;
        _pathHelperService = pathHelperService;
        _moduleConfigurationResolver = moduleConfigurationResolver;
    }

    protected async Task<(Configuration.Module.ModuleConfiguration? mergedModuleConfig, string? moduleConfigFileName)> GetModuleConfiguration(string? repoName, string moduleName)
    {
        var (repoSourceDirectory, _) = GetRepositorySourceAndTargetDirectory(repoName);

        var moduleNameAsPath = moduleName.Replace('/', Path.DirectorySeparatorChar);
        var moduleDirectory = Path.Combine(repoSourceDirectory, moduleNameAsPath);

        var moduleConfigFileStub = Path.Combine(moduleDirectory, Constants.ModuleConfigFileStub);
        return await _moduleConfigurationResolver.ResolveModuleConfiguration(moduleConfigFileStub);
    }

    protected (string? repoName, string? moduleName) GetRepositoryAndModuleName(List<string> parameters, bool singleParamIsRepo = false)
    {
        //TODO: handle parameters
        string? repositoryName = null;
        string? moduleName = null;

        parameters = parameters.Where(p => !p.StartsWith("-")).ToList();

        if (parameters.Count == 1)
        {
            if (singleParamIsRepo)
            {
                repositoryName = parameters[0];
            }
            else
            {
                moduleName = parameters[0];
            }
        }
        else if (parameters.Count >= 1)
        {
            repositoryName = parameters[0];
            moduleName = parameters[1];
        }

        return (repositoryName, moduleName);
    }

    protected (string repoSourceDirectory, string repoTargetDirectory) GetRepositorySourceAndTargetDirectory(string? repoName)
    {
        string repoSourceDirectory = Path.Combine(Environment.CurrentDirectory);
        string repoTargetDirectory = Path.Combine(Environment.CurrentDirectory, "..");
        return GetRepositorySourceAndTargetDirectory(repoName, repoSourceDirectory, repoTargetDirectory);
    }
    protected (string repoSourceDirectory, string repoTargetDirectory) GetRepositorySourceAndTargetDirectory(string? repoName, string fallbackSourceDirectory, string fallbackTargetDirectory)
    {
        if (repoName is not null
            && _repositoryConfiguration.Configuration.Repositories.Find(r => r.Name == repoName) is { } repoConfig)
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