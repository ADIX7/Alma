namespace Alma.Configuration.Repository;

public interface IRepositoryConfiguration
{
    public Task LoadAsync();
    RepositoryConfigurationRoot Configuration { get; }
}