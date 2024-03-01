

using Alma.Command.Install;
using Alma.Configuration.Repository;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.List;

public class ReadMeCommand : RepositoryModuleCommandBase
{
    private ILogger<InstallCommand> _logger;

    public override string CommandString => "readme";

    public override string[] CommandAliases => Array.Empty<string>();

    public ReadMeCommand(
        ILogger<InstallCommand> logger,
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IPathHelperService pathHelperService)
        : base(repositoryConfiguration, pathHelperService, moduleConfigurationResolver)
    {
        _logger = logger;
    }

    public override async Task Run(List<string> parameters)
    {
        var (repoName, moduleName) = GetRepositoryAndModuleName(parameters);
        if (moduleName is null)
        {
            _logger.LogInformation("No module specified");
            return;
        }

        var (repoSource, _) = GetRepositorySourceAndTargetDirectory(repoName);
        if (repoSource is null)
        {
            _logger.LogInformation("No repository source found");
            return;
        }

        var fileFound = false;
        var readmeMdPath = Path.Combine(repoSource, moduleName, "README.md");
        var readmeTxtPath = Path.Combine(repoSource, moduleName, "README.md");
        var readmePath = Path.Combine(repoSource, moduleName, "README.md");
        if(File.Exists(readmeMdPath))
        {
            fileFound = true;
            await PrintReadMeMd(readmeMdPath);
        }
        else if(File.Exists(readmeTxtPath))
        {
            fileFound = true;
            await PrintReadMeText(readmeMdPath);
        }
        else if(File.Exists(readmePath))
        {
            fileFound = true;
            await PrintReadMeText(readmePath);
        }

        if(!fileFound)
        {
            _logger.LogInformation("No README file found. Supported formats: README.md, README.txt, README");
        }
    }

    private async Task PrintReadMeMd(string filePath)
    {
        var content = await File.ReadAllLinesAsync(filePath);
        foreach(var line in content)
        {
            //TODO: Add support for markdown
            _logger.LogInformation(line);
        }
    }

    private async Task PrintReadMeText(string filePath)
    {
        var content = await File.ReadAllLinesAsync(filePath);
        foreach(var line in content)
        {
            _logger.LogInformation(line);
        }
    }
}