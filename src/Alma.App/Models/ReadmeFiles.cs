namespace Alma.Models;

public enum ReadmeFiles
{
    Markdown,
    Text,
    NoExtension
}

public static class ReadmeFileTypeExtensions
{
    public static string GetFileName(this ReadmeFiles type)
        => type switch
        {
            ReadmeFiles.Markdown => "README.md",
            ReadmeFiles.Text => "README.txt",
            ReadmeFiles.NoExtension => "README",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}