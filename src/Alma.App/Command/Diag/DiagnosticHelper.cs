namespace Alma.Command.Diag;

[AttributeUsage(AttributeTargets.Method)]
public class DiagnosticHelper : Attribute
{
    public string Command { get; }

    public DiagnosticHelper(string command)
    {
        Command = command;
    }
}