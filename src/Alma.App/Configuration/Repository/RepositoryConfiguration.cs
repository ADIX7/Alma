using Alma.Services;

namespace Alma.Configuration.Repository;

public class RepositoryConfiguration : IRepositoryConfiguration
{
    private readonly IFolderService _folderService;
    private readonly ConfigurationFileReader _configurationFileReader;

    public RepositoryConfigurationRoot Configuration { get; private set; } = new RepositoryConfigurationRoot(new List<RepositoryConfigurationEntry>());

    public RepositoryConfiguration(IFolderService folderService, ConfigurationFileReader configurationFileReader)
    {
        _folderService = folderService;
        _configurationFileReader = configurationFileReader;
    }

    public async Task LoadAsync()
    {
        if (_folderService.ConfigRoot is null)
        {
            Configuration = new RepositoryConfigurationRoot(new List<RepositoryConfigurationEntry>());
            return;
        }

        var repoConfigFileNameStub = Path.Combine(_folderService.ConfigRoot, "repository");
        var (configuration, repoConfigFileName) = await _configurationFileReader.DeserializeAsync<RepositoryConfigurationRoot>(repoConfigFileNameStub, (o) => new JsonSourceGenerationContext(o));
        Configuration = configuration ?? new RepositoryConfigurationRoot(new List<RepositoryConfigurationEntry>());

        foreach (var repositoryConfigurationEntry in Configuration.Repositories)
        {
            if (repositoryConfigurationEntry.Name is null)
            {
                Console.WriteLine($"Entry name is null in {repoConfigFileName}");
            }
        }
    }
}