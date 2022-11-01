namespace Alma.Services;

public interface IConfigurationFileReader
{
    public Task<(T? Result, string? FileName)> DeserializeAsync<T>(string fileNameWithoutExtension, string? extension = null) where T : class;
}