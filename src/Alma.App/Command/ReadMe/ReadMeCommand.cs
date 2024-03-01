

using Alma.Command.Install;
using Alma.Configuration.Repository;
using Alma.Logging;
using Alma.Models;
using Alma.Services;

namespace Alma.Command.List;

public class ReadMeCommand : RepositoryModuleCommandBase
{
    private ILogger<InstallCommand> _logger;

    public override string CommandString => "readme";

    public override string[] CommandAliases => Array.Empty<string>();

    private readonly Dictionary<ReadmeFiles, Func<string, Task>> _readmeFilePrinters;

    public ReadMeCommand(
        ILogger<InstallCommand> logger,
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IPathHelperService pathHelperService)
        : base(repositoryConfiguration, pathHelperService, moduleConfigurationResolver)
    {
        _logger = logger;

        _readmeFilePrinters = new Dictionary<ReadmeFiles, Func<string, Task>>
        {
            { ReadmeFiles.Markdown, PrintReadMeMd },
            { ReadmeFiles.Text, PrintReadMeText },
            { ReadmeFiles.NoExtension, PrintReadMeText },
        };
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

        foreach (var readmeFile in _readmeFilePrinters.Keys)
        {
            // TODO: make this case insensitive
            var readmeFilePath = Path.Combine(repoSource, moduleName, readmeFile.ToString());
            if (File.Exists(readmeFilePath))
            {
                fileFound = true;
                await _readmeFilePrinters[readmeFile](readmeFilePath);
                break;
            }
        }

        if (!fileFound)
        {
            _logger.LogInformation("No README file found. Supported formats: README.md, README.txt, README");
        }
    }

    private async Task PrintReadMeMd(string filePath)
    {
        var content = await File.ReadAllLinesAsync(filePath);
        foreach (var line in content)
        {
            //TODO: Add support for markdown
            _logger.LogInformation(line);
        }
    }

    private async Task PrintReadMeText(string filePath)
    {
        var content = await File.ReadAllLinesAsync(filePath);
        foreach (var line in content)
        {
            _logger.LogInformation(line);
        }
    }
}