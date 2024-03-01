namespace Alma.Configuration.Module;

public class ModuleConfiguration
{
    public string? Target { get; set; }
    public Dictionary<string, string>? Links { get; set; }
    public List<string>? Exclude { get; set; }
    public bool ExcludeReadme { get; set; } = true;

    public string? Install { get; set; }
    public string? Configure { get; set; }

    public ModuleConfiguration(string? target, Dictionary<string, string>? links, string? install, string? configure)
    {
        Target = target;
        Links = links;
        Install = install;
        Configure = configure;
    }

    public ModuleConfiguration Merge(ModuleConfiguration merge)
    {
        var mergedLinks = (Links ?? new Dictionary<string, string>())
            .Concat(merge.Links ?? new Dictionary<string, string>());
        return new ModuleConfiguration(
            merge.Target ?? Target,
            new Dictionary<string, string>(mergedLinks),
            merge.Install ?? Install,
            merge.Configure ?? Configure
        );
    }

    public static ModuleConfiguration Empty() =>
        new(null, new Dictionary<string, string>(), null, null);
}