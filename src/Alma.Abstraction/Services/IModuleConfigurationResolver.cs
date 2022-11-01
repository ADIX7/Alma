using Alma.Configuration.Module;

namespace Alma.Services;

public interface IModuleConfigurationResolver
{
    Task<(ModuleConfiguration? mergedModuleConfig, string? moduleConfigFileName)> ResolveModuleConfiguration(string moduleConfigStub);
}