using System.Diagnostics;
using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Install;

public class InstallCommand : RepositoryModuleCommandBase
{
    private readonly ILogger<InstallCommand> _logger;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly IShellService _shellService;
    public override string CommandString => "install";

    public InstallCommand(
        ILogger<InstallCommand> logger,
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IShellService shellService)
        : base(repositoryConfiguration)
    {
        _logger = logger;
        _moduleConfigurationResolver = moduleConfigurationResolver;
        _shellService = shellService;
    }

    public override async Task Run(List<string> parameters)
    {
        var (repoName, moduleName) = GetRepositoryAndModuleName(parameters);
        if (moduleName is null)
        {
            _logger.LogInformation("No module specified");
            return;
        }

        string sourceDirectory = Path.Combine(Environment.CurrentDirectory);
        string targetDirectory = Path.Combine(Environment.CurrentDirectory, "..");

        string moduleNameAsPath = moduleName.Replace('/', Path.DirectorySeparatorChar);
        (sourceDirectory, _) = GetModuleSourceAndTargetDirectory(repoName, sourceDirectory, targetDirectory);

        string moduleDirectory = Path.Combine(sourceDirectory, moduleNameAsPath);

        var moduleConfigFileStub = Path.Combine(moduleDirectory, Constants.ModuleConfigFileStub);
        var (moduleConfiguration, moduleConfigurationFile) = await _moduleConfigurationResolver.ResolveModuleConfiguration(moduleConfigFileStub);

        if (moduleConfiguration is null)
        {
            _logger.LogInformation($"No module configuration found for module '{moduleName}'{(repoName is null ? "" : $" in repository '{repoName}'")}");
            return;
        }

        var installLines = moduleConfiguration.Install?.Split(Environment.NewLine);

        if (installLines is null)
        {
            _logger.LogInformation("No install command is found");
            return;
        }

        _logger.LogInformation($"Install command: {string.Join("\n", installLines)}");

        if (installLines.Length == 1)
        {
            _logger.LogInformation("Running install command '" + installLines[0] + "'");
            await _shellService.RunCommandAsync(installLines[0]);
        }
        else
        {
            _logger.LogError("Multi line scripts are not currently supported");
        }
    }
}