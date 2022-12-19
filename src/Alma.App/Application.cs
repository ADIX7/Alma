using Alma.Command;
using Alma.Command.Help;
using Alma.Logging;

namespace Alma;

public class Application
{
    private readonly IList<ICommand> _commands;
    private readonly ILogger<Application> _logger;

    public Application(IEnumerable<ICommand> commands, ILogger<Application> logger)
    {
        _commands = commands.ToList();
        _logger = logger;
    }

    public async Task Run(string[] args)
    {
        if (args.Length == 0)
        {
            _logger.LogInformation("No command was given");
            return;
        }

        var commandString = args[0];

        var command = _commands.FirstOrDefault(c => c.CommandString == commandString);

        if (command is null)
        {
            _logger.LogInformation($"Invalid command: {commandString}");
            return;
        }

        await command.Run(args[1..].ToList());

        return;
    }
}