using System.Runtime.InteropServices;
using Alma.Configuration.Module;
using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Models;
using Alma.Services;

namespace Alma.Command.Link;

public class LinkCommand : RepositoryModuleCommandBase
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly IMetadataHandler _metadataHandler;
    private readonly IPathHelperService _pathHelperService;
    private readonly ILogger<LinkCommand> _logger;

    public override string CommandString => "link";
    public override string[] CommandAliases => Array.Empty<string>();

    public LinkCommand(
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IMetadataHandler metadataHandler,
        IPathHelperService pathHelperService,
        ILogger<LinkCommand> logger)
        : base(repositoryConfiguration, pathHelperService, moduleConfigurationResolver)
    {
        _repositoryConfiguration = repositoryConfiguration;
        _moduleConfigurationResolver = moduleConfigurationResolver;
        _metadataHandler = metadataHandler;
        _pathHelperService = pathHelperService;
        _logger = logger;
    }

    public override async Task Run(List<string> parameters)
    {
        if (parameters.Contains("--help"))
        {
            _logger.LogInformation(
                """
                Usage:                
                    alma link [module]
                    alma link [repository] [module]

                Options:
                    --help          Show this message
                    -d, --dry-run   Show what would be linked without actually linking
                """
            );
            return;
        }

        var dryRun = parameters.Contains("-d") || parameters.Contains("--dry-run");

        var (repoName, moduleName) = GetRepositoryAndModuleName(parameters);
        if (moduleName is null)
        {
            _logger.LogInformation("No module specified");
            return;
        }

        var (sourceDirectory, targetDirectory) = GetRepositorySourceAndTargetDirectory(repoName);

        if (!Directory.Exists(sourceDirectory))
        {
            _logger.LogInformation("Source directory not exists: " + sourceDirectory);
            return;
        }

        string moduleNameAsPath = moduleName.Replace('/', Path.DirectorySeparatorChar);
        string moduleDirectory = Path.Combine(sourceDirectory, moduleNameAsPath);

        if (!Directory.Exists(moduleDirectory))
        {
            _logger.LogInformation("Module directory not exists: " + moduleDirectory);
            return;
        }

        var moduleConfigFileStub = Path.Combine(moduleDirectory, Constants.ModuleConfigFileStub);
        var (moduleConfiguration, moduleConfigurationFile) = await _moduleConfigurationResolver.ResolveModuleConfiguration(moduleConfigFileStub);

        if (moduleConfiguration?.Target is string moduleTargetDir)
        {
            targetDirectory = _pathHelperService.ResolvePath(moduleTargetDir, targetDirectory);
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var moduleConfigurationFileFullPath = moduleConfigurationFile is null ? null : Path.Combine(moduleDirectory, moduleConfigurationFile);

        var moduleDir = new DirectoryInfo(moduleDirectory);
        var currentTargetDirectory = new DirectoryInfo(targetDirectory);
        var itemsToLink = (await TraverseTree(
            moduleDir,
            currentTargetDirectory,
            moduleDir,
            currentTargetDirectory,
            moduleConfiguration)).ToList();

        // Exclude
        if (moduleConfigurationFile is not null) itemsToLink.RemoveAll(i => i.SourcePath == moduleConfigurationFileFullPath);

        if (moduleConfiguration?.Exclude is { } excludeList)
        {
            foreach (var itemToExclude in excludeList)
            {
                var excludePath = Path.Combine(moduleDirectory, Path.Combine(itemToExclude.Split('/')));
                itemsToLink.RemoveAll(
                    i => i.SourcePath == excludePath
                    || i.SourcePath.StartsWith(excludePath + Path.DirectorySeparatorChar)
                );
            }
        }

        if (moduleConfiguration?.ExcludeReadme ?? false)
        {
            foreach (var readmeFile in Enum.GetValues<ReadmeFiles>())
            {
                var readmeFilePath = Path.Combine(moduleDirectory, readmeFile.GetFileName());
                itemsToLink.RemoveAll(i => i.SourcePath == readmeFilePath);
            }
        }

        // Linking

        if (dryRun)
        {
            _logger.LogInformation("Dry run. No links will be created. The following links would be created:");
        }

        var successfulLinks = CreateLinks(itemsToLink, dryRun);

        if (dryRun) return;

        await _metadataHandler.SaveLinkedItemsAsync(successfulLinks, moduleDir, currentTargetDirectory);
    }

    private List<ItemToLink> CreateLinks(List<ItemToLink> itemsToLink, bool dryRun)
    {
        var successfulLinks = new List<ItemToLink>();

        try
        {
            foreach (var itemToLink in itemsToLink)
            {
                if (File.Exists(itemToLink.TargetPath) || Directory.Exists(itemToLink.TargetPath))
                {
                    _logger.LogInformation("Item already exists: " + itemToLink.TargetPath);
                    continue;
                }

                var sourceFileExists = File.Exists(itemToLink.SourcePath);
                var sourceDirectoryExists = Directory.Exists(itemToLink.SourcePath);

                _logger.LogInformation($"Linking: '{itemToLink.SourcePath}' '{itemToLink.TargetPath}'");

                if (!dryRun)
                {
                    if (sourceFileExists)
                    {
                        File.CreateSymbolicLink(itemToLink.TargetPath, itemToLink.SourcePath);
                    }
                    else if (sourceDirectoryExists)
                    {
                        Directory.CreateSymbolicLink(itemToLink.TargetPath, itemToLink.SourcePath);
                    }
                    else
                    {
                        _logger.LogInformation("Source not exists: " + itemToLink.SourcePath);
                        continue;
                    }
                }

                successfulLinks.Add(itemToLink);
            }
        }
        catch (IOException e)
        {
            _logger.LogInformation("An error occured while creating links: " + e.Message);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogInformation("On Windows symlinks can be created only with Administrator privileges.");
            }
        }

        return successfulLinks;
    }

    private async Task<IEnumerable<ItemToLink>> TraverseTree(
        DirectoryInfo currentDirectory,
        DirectoryInfo currentTargetDirectory,
        DirectoryInfo moduleDirectory,
        DirectoryInfo targetDirectory,
        ModuleConfiguration? moduleConfiguration)
    {
        var filesToLink = new List<ItemToLink>();
        foreach (var file in currentDirectory.GetFiles())
        {
            if (moduleConfiguration?.Links?.ContainsKey(file.Name) ?? false)
            {
                filesToLink.Add(new ItemToLink(file.FullName, _pathHelperService.ResolvePath(moduleConfiguration.Links[file.Name], targetDirectory.FullName)));
            }
            else
            {
                filesToLink.Add(new ItemToLink(file.FullName, Path.Combine(currentTargetDirectory.FullName, file.Name)));
            }
        }

        var subDirLinksToAdd = Enumerable.Empty<ItemToLink>();

        foreach (var subDir in currentDirectory.GetDirectories())
        {
            var relativePath = GetRelativePath(subDir.FullName, moduleDirectory.FullName);
            if (moduleConfiguration?.Links?.ContainsKey(relativePath) ?? false)
            {
                filesToLink.Add(new ItemToLink(subDir.FullName, _pathHelperService.ResolvePath(moduleConfiguration.Links[relativePath], targetDirectory.FullName)));
            }
            else
            {
                var subDirLinks = await TraverseTree(
                    subDir,
                    new DirectoryInfo(Path.Combine(currentTargetDirectory.FullName, subDir.Name)),
                    moduleDirectory,
                    targetDirectory,
                    moduleConfiguration
                );
                subDirLinksToAdd = subDirLinksToAdd.Concat(subDirLinks);
            }
        }

        return filesToLink.Concat(subDirLinksToAdd);
    }

    private static string GetRelativePath(string full, string parent) => full[parent.Length..].TrimStart(Path.DirectorySeparatorChar);
}