namespace Alma.Helper;

public static class PathHelper
{
    public static string ResolvePath(string path, string? currentDirectory = null)
    {
        var skipCombiningCurrentDirectory = false;
        if (path.StartsWith("~"))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = path.Length > 1 ? Path.Combine(userProfile, path[2..]) : userProfile;
            skipCombiningCurrentDirectory = true;
        }

        //TODO: more special character

        return currentDirectory is null || skipCombiningCurrentDirectory
            ? path
            : Path.Combine(currentDirectory, path);
    }
}