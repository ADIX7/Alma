namespace Alma.Command;

public interface ICommand
{
    string CommandString { get; }
    string[] CommandAliases { get; }
    Task Run(List<string> parameters);
}