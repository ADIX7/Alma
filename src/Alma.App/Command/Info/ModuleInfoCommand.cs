namespace Alma.Command.Info;

public class ModuleInfoCommand : ICommand
{
    public string CommandString => "info";
    public Task Run(List<string> parameters)
    {
        throw new NotImplementedException();
    }
}