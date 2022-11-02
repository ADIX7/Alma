namespace Alma.Command.Help;

public class HelpCommand : ICommand
{
    private readonly Func<IEnumerable<ICommand>> _commandsProvider;
    public string CommandString => "help";

    public HelpCommand(Func<IEnumerable<ICommand>> commandsProvider)
    {
        _commandsProvider = commandsProvider;
    }

    public Task Run(List<string> parameters)
    {
        Console.WriteLine("Commands:" + Environment.NewLine);

        foreach (var command in _commandsProvider().OrderBy(c => c.CommandString))
        {
            Console.WriteLine(command.CommandString);
        }

        return Task.CompletedTask;
    }
}