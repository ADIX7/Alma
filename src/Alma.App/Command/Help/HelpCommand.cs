using Alma.Logging;

namespace Alma.Command.Help;

public class HelpCommand : ICommand
{
    private readonly Func<IEnumerable<ICommand>> _commandsProvider;
    private readonly ILogger<HelpCommand> _logger;

    public string CommandString => "help";

    public HelpCommand(
        Func<IEnumerable<ICommand>> commandsProvider,
        ILogger<HelpCommand> logger
    )
    {
        _commandsProvider = commandsProvider;
        _logger = logger;
    }

    public Task Run(List<string> parameters)
    {
        _logger.LogInformation("Commands:" + Environment.NewLine);

        foreach (var command in _commandsProvider().OrderBy(c => c.CommandString))
        {
            _logger.LogInformation(command.CommandString);
        }

        return Task.CompletedTask;
    }
}