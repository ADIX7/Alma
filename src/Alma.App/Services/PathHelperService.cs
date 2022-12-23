using Alma.Data;

namespace Alma.Services;

public class PathHelperService : IPathHelperService
{
    private static List<SpecialPathResolver> _specialPathResolvers = new()
    {
        new("~", () => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), true),
        new("%DOCUMENTS%", () => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
    };

    public string ResolvePath(string path, string? currentDirectory = null)
    {
        var skipCombiningCurrentDirectory = false;
        /*if (path.StartsWith("~"))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = path.Length > 1 ? Path.Combine(userProfile, path[2..]) : userProfile;
            skipCombiningCurrentDirectory = true;
        }*/

        foreach (var specialPathResolver in _specialPathResolvers)
        {
            if (path.Contains(specialPathResolver.PathName))
            {
                var parts = path.Split(specialPathResolver.PathName);
                path = string.Join(specialPathResolver.Resolver(), parts);
                skipCombiningCurrentDirectory = (specialPathResolver.SkipCombiningCurrentDirectory ?? false) || skipCombiningCurrentDirectory;
            }
        }

        //TODO: more special character

        return currentDirectory is null || skipCombiningCurrentDirectory
            ? path
            : Path.Combine(currentDirectory, path);
    }
}