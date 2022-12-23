namespace Alma.Services;

public interface IPathHelperService
{
    string ResolvePath(string path, string? currentDirectory = null);
}