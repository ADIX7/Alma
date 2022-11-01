using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alma.Services;

public class ConfigurationFileReader
{
    private readonly List<IConfigurationFileReader> _configurationFileReaders;

    public ConfigurationFileReader(IEnumerable<IConfigurationFileReader> configurationFileReaders)
    {
        _configurationFileReaders = configurationFileReaders.ToList();
    }

    public async Task<(T? Result, string? FileName)> DeserializeAsync<T>(
        string fileNameWithoutExtension,
        Func<JsonSerializerOptions, JsonSerializerContext> contextGenerator,
        string? extension = null) where T : class
    {
        foreach (var configurationFileReader in _configurationFileReaders)
        {
            if (await configurationFileReader.DeserializeAsync<T>(fileNameWithoutExtension, contextGenerator, extension) is { Result: { } } result) return result;
        }

        return (null, null);
    }
}