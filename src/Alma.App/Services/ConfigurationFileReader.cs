namespace Alma.Services;

public class ConfigurationFileReader
{
    private readonly List<IConfigurationFileReader> _configurationFileReaders;

    public ConfigurationFileReader(IEnumerable<IConfigurationFileReader> configurationFileReaders)
    {
        _configurationFileReaders = configurationFileReaders.ToList();
    }

    public async Task<(T? Result, string? FileName)> DeserializeAsync<T>(string fileNameWithoutExtension, string? extension = null) where T : class
    {
        foreach (var configurationFileReader in _configurationFileReaders)
        {
            if (await configurationFileReader.DeserializeAsync<T>(fileNameWithoutExtension, extension) is {Result: { }} result) return result;
        }

        return (null, null);
    }
}