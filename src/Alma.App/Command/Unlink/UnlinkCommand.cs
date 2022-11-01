namespace Alma.Command.Unlink;

public class UnlinkCommand : ICommand
{
    public string CommandString => "unlink";
    public Task Run(List<string> parameters)
    {
        throw new NotImplementedException();
    }
}