namespace Alma.Configuration.Repository;

public class RepositoryConfigurationEntry
{
    public string Name { get; set; }
    public string? RepositoryPath { get; set; }
    public string? LinkPath { get; set; }

    public RepositoryConfigurationEntry(string name, string? repositoryPath, string? linkPath)
    {
        Name = name;
        RepositoryPath = repositoryPath;
        LinkPath = linkPath;
    }
}