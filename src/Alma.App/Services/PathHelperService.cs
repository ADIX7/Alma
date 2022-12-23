using Alma.Data;

namespace Alma.Services;

public class PathHelperService : IPathHelperService
{
    private static readonly List<SpecialPathResolver> _specialPathResolvers = new()
    {
        new("~", () => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), true),
        new("%DOCUMENTS%", () => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
    };

    public string ResolvePath(string path, string? currentDirectory = null)
    {
        var skipCombiningCurrentDirectory = false;

        foreach (var specialPathResolver in _specialPathResolvers)
        {
            if (path.Contains(specialPathResolver.PathName))
            {
                var parts = path.Split(specialPathResolver.PathName);
                path = string.Join(specialPathResolver.Resolver(), parts);
                skipCombiningCurrentDirectory = (specialPathResolver.SkipCombiningCurrentDirectory ?? false) || skipCombiningCurrentDirectory;
            }
        }

        path = path.Replace('/', Path.DirectorySeparatorChar);

        return currentDirectory is null || skipCombiningCurrentDirectory
            ? path
            : Path.Combine(currentDirectory, path);
    }
}