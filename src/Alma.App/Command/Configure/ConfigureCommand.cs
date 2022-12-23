using Alma.Command.Install;
using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Configure;

public class ConfigureCommand : RepositoryModuleCommandBase
{
    private readonly ILogger<InstallCommand> _logger;
    private readonly IShellService _shellService;
    public override string CommandString => "configure";

    public ConfigureCommand(
        ILogger<InstallCommand> logger,
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IShellService shellService,
        IPathHelperService pathHelperService)
        : base(repositoryConfiguration, pathHelperService, moduleConfigurationResolver)
    {
        _logger = logger;
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
        var (moduleConfiguration, _) = await GetModuleConfiguration(repoName, moduleName);

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