using Alma.Configuration.Module;

namespace Alma.Services;

public class ModuleConfigurationResolver : IModuleConfigurationResolver
{
    private readonly IConfigurationFileReader _configurationFileReader;
    private readonly IOsInformation _osInformation;

    public ModuleConfigurationResolver(
        IConfigurationFileReader configurationFileReader,
        IOsInformation osInformation)
    {
        _configurationFileReader = configurationFileReader;
        _osInformation = osInformation;
    }

    public async Task<(ModuleConfiguration? mergedModuleConfig, string? moduleConfigFileName)> ResolveModuleConfiguration(string moduleConfigStub)
    {
        (ModuleConfigurationRoot? moduleConfigRoot, string? moduleConfigFileName) = await _configurationFileReader.DeserializeAsync<ModuleConfigurationRoot>(moduleConfigStub, (o) => new JsonSourceGenerationContext(o));

        if (moduleConfigRoot is null) return (null, null);

        var validModuleConfigurations = await moduleConfigRoot
            .ToAsyncEnumerable()
            .WhereAwait(async m => await _osInformation.IsOnPlatformAsync(m.Key))
            .ToListAsync();

        //TODO: priority order
        var orderedValidModuleConfigurations = new Dictionary<string, ModuleConfiguration>(validModuleConfigurations);

        if (orderedValidModuleConfigurations.Count == 0)
        {
            return (ModuleConfiguration.Empty(), moduleConfigFileName);
        }

        var mergedModuleConfig = orderedValidModuleConfigurations
            .Select(m => m.Value)
            .Aggregate((a, b) => a.Merge(b));

        return (mergedModuleConfig, moduleConfigFileName);
    }
}