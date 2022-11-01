using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alma.Services;

public interface IConfigurationFileReader
{
    public Task<(T? Result, string? FileName)> DeserializeAsync<T>(
        string fileNameWithoutExtension,
        Func<JsonSerializerOptions, JsonSerializerContext> contextGenerator,
        string? extension = null)
    where T : class;
}