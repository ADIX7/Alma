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
        var (moduleConfigRoot, moduleConfigFileName) = await _configurationFileReader.DeserializeAsync<ModuleConfigurationRoot>(moduleConfigStub);

        if (moduleConfigRoot is null) return (null, null);

        var validModuleConfigurations = moduleConfigRoot.Where(m => _osInformation.IsOnPlatform(m.Key));
        
        //TODO: priority order
        var orderedValidModuleConfigurations = new Dictionary<string, ModuleConfiguration>(validModuleConfigurations);

        var mergedModuleConfig = orderedValidModuleConfigurations
            .Select(m => m.Value)
            .Aggregate((a, b) => a.Merge(b));

        return (mergedModuleConfig, moduleConfigFileName);
    }
}