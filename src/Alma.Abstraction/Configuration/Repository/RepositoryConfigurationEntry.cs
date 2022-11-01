namespace Alma.Configuration.Repository;

public record RepositoryConfigurationEntry(
    string Name,
    string? RepositoryPath,
    string? LinkPath
);