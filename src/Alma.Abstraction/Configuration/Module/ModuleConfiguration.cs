namespace Alma.Configuration.Module;

public record ModuleConfiguration(string? Target, Dictionary<string, string>? Links)
{
    public ModuleConfiguration Merge(ModuleConfiguration merge)
    {
        var mergedLinks = (Links ?? new Dictionary<string, string>())
            .Concat(merge.Links ?? new Dictionary<string, string>());
        return new ModuleConfiguration(
            merge.Target ?? Target,
            new Dictionary<string, string>(mergedLinks)
        );
    }

    public static ModuleConfiguration Empty() =>
        new(null, new Dictionary<string, string>());
}