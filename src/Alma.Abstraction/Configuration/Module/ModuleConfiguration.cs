namespace Alma.Configuration.Module;

public class ModuleConfiguration
{
    public string? Target { get; set; }
    public Dictionary<string, string>? Links { get; set; }

    public ModuleConfiguration(string? target, Dictionary<string, string>? links)
    {
        Target = target;
        Links = links;
    }

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