namespace Alma.Services;

public interface IFolderService
{
    string? ConfigRoot { get;  }
    string AppData { get; }
}