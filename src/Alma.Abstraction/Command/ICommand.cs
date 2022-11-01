namespace Alma.Command;

public interface ICommand
{
    public string CommandString { get; }
    public Task Run(List<string> parameters);
}