using Alma.Command;
using Alma.Command.Help;

namespace Alma;

public class Application
{
    private readonly IList<ICommand> _commands;

    public Application(IEnumerable<ICommand> commands)
    {
        _commands = commands.Append(new HelpCommand(() => _commands!)).ToList();
    }

    public async Task Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No command was given");
            return;
        }

        var commandString = args[0];

        var command = _commands.FirstOrDefault(c => c.CommandString == commandString);

        if (command is null)
        {
            Console.WriteLine($"Invalid command: {commandString}");
            return;
        }

        await command.Run(args[1..].ToList());

        return;
    }
}