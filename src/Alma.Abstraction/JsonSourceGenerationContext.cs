using System.Text.Json.Serialization;
using Alma.Configuration.Module;
using Alma.Configuration.Repository;

namespace Alma;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ModuleConfigurationRoot))]
[JsonSerializable(typeof(ModuleConfiguration))]
[JsonSerializable(typeof(RepositoryConfigurationEntry))]
[JsonSerializable(typeof(RepositoryConfigurationRoot))]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
