using System.Runtime.InteropServices;
using Alma.Configuration.Module;
using Alma.Configuration.Repository;
using Alma.Data;
using Alma.Logging;
using Alma.Services;

namespace Alma.Command.Link;

public class LinkCommand : ICommand
{
    private readonly IRepositoryConfiguration _repositoryConfiguration;
    private readonly IModuleConfigurationResolver _moduleConfigurationResolver;
    private readonly IMetadataHandler _metadataHandler;
    private readonly ILogger<LinkCommand> _logger;

    public string CommandString => "link";

    public LinkCommand(
        IRepositoryConfiguration repositoryConfiguration,
        IModuleConfigurationResolver moduleConfigurationResolver,
        IMetadataHandler metadataHandler,
        ILogger<LinkCommand> logger)
    {
        _repositoryConfiguration = repositoryConfiguration;
        _moduleConfigurationResolver = moduleConfigurationResolver;
        _metadataHandler = metadataHandler;
        _logger = logger;
    }

    public async Task Run(List<string> parameters)
    {
        if (parameters.Count == 0)
        {
            _logger.LogInformation("No module specified");
            return;
        }

        string moduleName = parameters[0];

        string sourceDirectory = Path.Combine(Environment.CurrentDirectory);
        string targetDirectory = Path.Combine(Environment.CurrentDirectory, "..");

        var repoName = GetRepositoryName(parameters);
        if (repoName is not null
            && _repositoryConfiguration.Configuration.Repositories.FirstOrDefault(r => r.Name == repoName) is { } repoConfig)
        {
            sourceDirectory = repoConfig.RepositoryPath ?? sourceDirectory;
            targetDirectory = repoConfig.LinkPath ?? targetDirectory;
        }

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
            targetDirectory = ResolvePath(moduleTargetDir, targetDirectory);
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
        if (moduleConfigurationFile is not null) itemsToLink.RemoveAll(i => i.SourcePath == moduleConfigurationFileFullPath);

        var successfulLinks = CreateLinks(itemsToLink);

        await _metadataHandler.SaveLinkedItemsAsync(successfulLinks, moduleDir, currentTargetDirectory);
    }

    private List<ItemToLink> CreateLinks(List<ItemToLink> itemsToLink)
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

                successfulLinks.Add(itemToLink);
            }
        }
        catch (IOException e)
        {
            _logger.LogInformation("An error occured while creating links: " + e.Message);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogInformation("On Windows symlinks can be greated only with Administrator privileges.");
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
            filesToLink.Add(new ItemToLink(Path.Combine(currentDirectory.FullName, file.Name), Path.Combine(currentTargetDirectory.FullName, file.Name)));
        }

        var subDirLinksToAdd = Enumerable.Empty<ItemToLink>();

        foreach (var subDir in currentDirectory.GetDirectories())
        {
            var relativePath = GetRelativePath(subDir.FullName, moduleDirectory.FullName);
            if (moduleConfiguration?.Links?.ContainsKey(relativePath) ?? false)
            {
                filesToLink.Add(new ItemToLink(subDir.FullName, ResolvePath(moduleConfiguration.Links[relativePath], targetDirectory.FullName)));
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

    private static string? GetRepositoryName(List<string> parameters)
    {
        //TODO: handle parameters
        if (parameters.Count < 2) return null;
        return parameters[1];
    }

    private static string ResolvePath(string path, string currentDirectory)
    {
        if (path.StartsWith("~"))
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Substring(2));
        }

        //TODO: more special character

        return Path.Combine(currentDirectory, path);
    }

    private static string GetRelativePath(string full, string parent) => full.Substring(parent.Length).TrimStart(Path.DirectorySeparatorChar);
}