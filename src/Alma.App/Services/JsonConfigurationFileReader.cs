using System.Text.Json;

namespace Alma.Services;

public class JsonConfigurationFileReader : IConfigurationFileReader
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    public async Task<(T? Result, string? FileName)> DeserializeAsync<T>(string fileNameWithoutExtension, string? extension) where T : class
    {
        extension ??= "json";
        var fileName = fileNameWithoutExtension + "." + extension;
        if (!File.Exists(fileName)) return (null, null);

        await using FileStream openStream = File.OpenRead(fileName);
        return (await JsonSerializer.DeserializeAsync<T>(openStream, DefaultOptions), fileName);
    }
}