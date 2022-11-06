namespace Alma.Services;

public interface IShellService
{
    Task RunCommandAsync(string command);
}