using System.Reflection;
using Alma.Logging;

namespace Alma.Command.Diag;

public class DiagCommand : ICommand
{
    private readonly ILogger<DiagCommand> _logger;
    public string CommandString => "diag";
    private readonly Lazy<IReadOnlyList<MethodInfo>> _diagnosticHelpersLazy;

    public DiagCommand(ILogger<DiagCommand> logger)
    {
        _logger = logger;
        _diagnosticHelpersLazy = new Lazy<IReadOnlyList<MethodInfo>>(FindDiagnosticHelpers);
    }

    public Task Run(List<string> parameters)
    {
        if (parameters.Count < 1)
        {
            _logger.LogInformation("No diagnostic helper specified.");
            return Task.CompletedTask;
        }

        var command = parameters[0];

        if (command == "list")
        {
            ListDiagnosticHelpers();
            return Task.CompletedTask;
        }

        var diagnosticHelpers = _diagnosticHelpersLazy.Value;

        var helper = diagnosticHelpers.FirstOrDefault(
            h => GetDiagnosticHelper(h) is { } attr && attr.Command == command
        );

        if (helper is null)
        {
            _logger.LogInformation($"Diagnostic helper {command} is not found.");
            return Task.CompletedTask;
        }

        if (!helper.IsStatic)
        {
            _logger.LogInformation($"Diagnostic helper {helper.Name} is not static.");
            return Task.CompletedTask;
        }

        HandleHelper(helper, parameters.Skip(1));

        return Task.CompletedTask;
    }

    private void ListDiagnosticHelpers()
    {
        var diagnosticHelpers = _diagnosticHelpersLazy.Value;
        var commands = diagnosticHelpers
            .Select(h => GetDiagnosticHelper(h)?.Command)
            .OfType<string>()
            .ToList();

        _logger.LogInformation("Available diagnostic helpers:");
        foreach (var command in commands)
        {
            _logger.LogInformation(command);
        }
    }

    private void HandleHelper(MethodInfo helper, IEnumerable<string> parameters)
    {
        var helperParameters = helper.GetParameters();
        var helperArguments = new object[helperParameters.Length];
        for (var i = 0; i < helperParameters.Length; i++)
        {
            var parameterType = helperParameters[i].ParameterType;
            if (parameterType == typeof(IEnumerable<string>))
            {
                helperArguments[i] = parameters;
            }
            else if (parameterType == typeof(ILogger))
            {
                helperArguments[i] = _logger;
            }
            else
            {
                _logger.LogInformation($"Diagnostic helper {helper.Name} has wrong parameter type, could not resolve '{parameterType}'.");
                return;
            }
        }
        
        helper.Invoke(null, helperArguments);
    }

    private IReadOnlyList<MethodInfo> FindDiagnosticHelpers()
    {
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
        };

        return assemblies.SelectMany(
                a =>
                    a
                        .GetTypes()
                        .SelectMany(t => t.GetMethods())
                        .Where(t => t.GetCustomAttributes(typeof(DiagnosticHelper)).Any()))
            .ToList()
            .AsReadOnly();
    }

    private DiagnosticHelper? GetDiagnosticHelper(MethodInfo o)
        => o.GetCustomAttributes(typeof(DiagnosticHelper)).FirstOrDefault() as DiagnosticHelper;
}