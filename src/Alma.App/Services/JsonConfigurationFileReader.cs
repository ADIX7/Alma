using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alma.Services;

public class JsonConfigurationFileReader : IConfigurationFileReader
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    public async Task<(T? Result, string? FileName)> DeserializeAsync<T>(
        string fileNameWithoutExtension,
        Func<JsonSerializerOptions, JsonSerializerContext> contextGenerator,
        string? extension) where T : class
    {
        extension ??= "json";
        var fileName = fileNameWithoutExtension + "." + extension;
        if (!File.Exists(fileName)) return (null, null);

        await using FileStream openStream = File.OpenRead(fileName);
        var result = 
            (T?)await JsonSerializer.DeserializeAsync(
                openStream,
                typeof(T),
                contextGenerator(new JsonSerializerOptions(DefaultOptions))
            );
        return (result, fileName);
    }
}