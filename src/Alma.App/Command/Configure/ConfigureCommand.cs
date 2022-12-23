using Alma.Command.Install;
using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Configure;

public class ConfigureCommand : RepositoryModuleCommandBase
{
    private readonly ILogger<InstallCommand> _logger;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly IShellService _shellService;
    public override string CommandString => "configure";

    public ConfigureCommand(
        ILogger<InstallCommand> logger,
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IShellService shellService,
        IPathHelperService pathHelperService)
        : base(repositoryConfiguration, pathHelperService)
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

        var configureLines = moduleConfiguration.Configure?.Split(Environment.NewLine);

        if (configureLines is null)
        {
            _logger.LogInformation("No configure command is found");
            return;
        }

        _logger.LogInformation($"Configure command: {string.Join("\n", configureLines)}");

        if (configureLines.Length == 1)
        {
            _logger.LogInformation("Running configure command '" + configureLines[0] + "'");
            await _shellService.RunCommandAsync(configureLines[0]);
        }
        else
        {
            _logger.LogError("Multi line scripts are not currently supported");
        }
    }
}