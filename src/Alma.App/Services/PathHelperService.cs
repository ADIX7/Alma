using System.Diagnostics.CodeAnalysis;
using Alma.Command.Diag;
using Alma.Data;
using Alma.Logging;

namespace Alma.Services;

public class PathHelperService : IPathHelperService
{
    private static readonly List<SpecialPathResolver> _specialPathResolvers = new()
    {
        new("~", () => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), true),
        new("%DOCUMENTS%", () => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
    };

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PathHelperService))]
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

    [DiagnosticHelper("special-path-resolver")]
    public static void SpecialPathResolverDiag(ILogger logger)
    {
        logger.LogInformation($"There are {_specialPathResolvers.Count} special path resolvers:");
        foreach (var specialPathResolver in _specialPathResolvers)
        {
            logger.LogInformation($"{specialPathResolver.PathName} => {specialPathResolver.Resolver()}");
        }
    }
}