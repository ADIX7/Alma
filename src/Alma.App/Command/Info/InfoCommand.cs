using Alma.Configuration.Module;
using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Info;

public class InfoCommand : RepositoryModuleCommandBase
{
    public override string CommandString => "info";

    private readonly IFolderService _folderService;
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly ILogger<InfoCommand> _logger;
    private readonly IOsInformation _osInformation;
    private readonly IVersionService _versionService;
    private readonly IPathHelperService _pathHelperService;

    private static readonly List<Func<ModuleConfiguration, string?>> _moduleInfoDetailResolvers = new()
    {
        (m) =>
        {
            var linkCount = m.Links?.Count ?? 0;
            return linkCount.ToString().PadLeft(3) + $" link{(linkCount > 1 ? "s" : "")}".PadRight(6);
        },
        (m) => m.Install is not null ? "[Install]" : null,
        (m) => m.Configure is not null ? "[Configure]" : null,
    };

    public InfoCommand(
        IFolderService folderService,
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        ILogger<InfoCommand> logger,
        IOsInformation osInformation,
        IVersionService versionService,
        IPathHelperService pathHelperService
    ) : base(repositoryConfiguration, pathHelperService, moduleConfigurationResolver)
    {
        _folderService = folderService;
        _repositoryConfiguration = repositoryConfiguration;
        _moduleConfigurationResolver = moduleConfigurationResolver;
        _logger = logger;
        _osInformation = osInformation;
        _versionService = versionService;
        _pathHelperService = pathHelperService;
    }

    public override async Task Run(List<string> parameters)
    {
        var (repoName, moduleName) = GetRepositoryAndModuleName(parameters, true);
        if (repoName is not null && moduleName is null)
        {
            await ProcessRepoInfoAsync(repoName);
        }
        else if (repoName is not null && moduleName is not null)
        {
            await ProcessModuleInfoAsync(repoName, moduleName);
        }
        else
        {
            await ProcessGeneralInfoAsync();
        }
    }

    private async Task ProcessGeneralInfoAsync()
    {
        _logger.LogInformation("Alma " + _versionService.GetVersion());
        _logger.LogInformation("");

        _logger.LogInformation("AppData folder: " + _folderService.AppData);

        if (_folderService.ConfigRoot is { } configRoot)
        {
            _logger.LogInformation("Configuration folder: " + configRoot);
        }
        else
        {
            _logger.LogInformation("Configuration folder not exists.");
            _logger.LogInformation("Preferred configuration folder is: " + Path.Combine(_folderService.GetPreferredConfigurationFolder(), _folderService.ApplicationSubfolderName));
        }

        _logger.LogInformation("");
        _logger.LogInformation($"Platform is '{await _osInformation.GetOsIdentifierAsync()}'");
        _logger.LogInformation("");

        if (_repositoryConfiguration.Configuration.Repositories is { Count: > 0 } repositories)
        {
            _logger.LogInformation("Repositories:");
            foreach (var repository in repositories)
            {
                Console.Write(repository.Name);
                if (repository.RepositoryPath is not null && !Directory.Exists(_pathHelperService.ResolvePath(repository.RepositoryPath)))
                {
                    Console.Write($" (containing folder not exists {repository.RepositoryPath})");
                }

                _logger.LogInformation("");
            }
        }
        else
        {
            _logger.LogInformation("No repositories found");
        }
    }

    private async Task ProcessRepoInfoAsync(string repoName)
    {
        var (repoSourceDirectory, _) = GetRepositorySourceAndTargetDirectory(repoName);

        var repoRoot = new DirectoryInfo(repoSourceDirectory);
        var modules = (await TraverseRepoFolder(repoRoot, repoRoot)).OrderBy(e => e.Name).ToList();

        var maxNameLength = modules.Max(m => m.Name.Length);

        _logger.LogInformation($"Repository '{repoName}' contains {modules.Count} modules:");
        _logger.LogInformation("");

        foreach (var module in modules)
        {
            var moduleDetails = _moduleInfoDetailResolvers
                .Select(m => m(module.Configuration))
                .Where(m => m is not null)
                .ToList();
            _logger.LogInformation($"{module.Name.PadRight(maxNameLength + 3)} {string.Join("    ", moduleDetails)}");
        }
    }

    async Task<IEnumerable<ModuleConfigurationWithName>> TraverseRepoFolder(DirectoryInfo repoRoot, DirectoryInfo currentDirectory)
    {
        var modulesFound = Enumerable.Empty<ModuleConfigurationWithName>();

        var moduleConfigFileStub = Path.Combine(currentDirectory.FullName, Constants.ModuleConfigFileStub);
        var (moduleConfig, _) = await _moduleConfigurationResolver.ResolveModuleConfiguration(moduleConfigFileStub);

        if (moduleConfig is not null)
        {
            var moduleName = currentDirectory.FullName[(repoRoot.FullName.TrimEnd(Path.DirectorySeparatorChar).Length + 1)..].Replace(Path.DirectorySeparatorChar, '/');
            modulesFound = modulesFound.Append(new(moduleName, moduleConfig));
        }

        foreach (var subDir in currentDirectory.GetDirectories())
        {
            modulesFound = modulesFound.Concat(await TraverseRepoFolder(repoRoot, subDir));
        }

        return modulesFound;
    }

    async Task ProcessModuleInfoAsync(string repoName, string moduleName)
    {
        var (moduleConfiguration, moduleConfigFileName) = await GetModuleConfiguration(repoName, moduleName);

        if (moduleConfiguration is null)
        {
            _logger.LogInformation($"No configuration is found for module '{moduleName}' in repository '{repoName}':");
            return;
        }

        _logger.LogInformation($"Information about module '{moduleName}' in repository '{repoName}':");
        _logger.LogInformation("");

        var moduleTargetPath = moduleConfiguration.Target is not null
            ? _pathHelperService.ResolvePath(moduleConfiguration.Target)
            : null;

        if (moduleTargetPath is not null)
        {
            _logger.LogInformation($"Target directory is: {moduleTargetPath}");
            _logger.LogInformation("");
        }

        if (moduleConfiguration.Install is not null)
        {
            _logger.LogInformation("Can be installed.");
        }

        if (moduleConfiguration.Configure is not null)
        {
            _logger.LogInformation("Can be configured.");
        }

        if (moduleConfiguration.Links is { } links && links.Count != 0)
        {
            var linkCount = links.Count;
            _logger.LogInformation("");
            _logger.LogInformation($"Has {linkCount} link{(linkCount > 1 ? "s" : "")}:");
            _logger.LogInformation("");

            foreach (var link in links)
            {
                var sourcePath = Path.Combine(new FileInfo(moduleConfigFileName!).Directory!.FullName, link.Key);
                var sourceExists = File.Exists(sourcePath) || Directory.Exists(sourcePath);
                var sourceColor = sourceExists ? ColorCodes.GreenForeground : ColorCodes.RedForeground;

                var targetColor = ColorCodes.RedForeground;
                if (moduleTargetPath is not null)
                {
                    var targetPath = Path.Combine(moduleTargetPath, link.Key);
                    var targetExists = File.Exists(targetPath) || Directory.Exists(targetPath);

                    targetColor = targetExists ? ColorCodes.GreenForeground : ColorCodes.RedForeground;
                }

                _logger.LogInformation($"{sourceColor}{link.Key}{ColorCodes.Reset} -> {targetColor}{link.Value}");
            }
        }
        else
        {
            _logger.LogInformation("Has no links.");
        }
    }
}
