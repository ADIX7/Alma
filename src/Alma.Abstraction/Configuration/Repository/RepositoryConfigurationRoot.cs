namespace Alma.Configuration.Repository;

public record RepositoryConfigurationRoot
{
    public List<RepositoryConfigurationEntry> Repositories { get; set; }

    public RepositoryConfigurationRoot(List<RepositoryConfigurationEntry> repositories)
    {
        Repositories = repositories;
    }
}