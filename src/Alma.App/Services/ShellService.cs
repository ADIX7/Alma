using System.Diagnostics;
using System.Runtime.InteropServices;
using Alma.Logging;

namespace Alma.Services;

public class ShellService : IShellService
{
    private readonly ILogger<ShellService> _logger;

    public ShellService(ILogger<ShellService> logger)
    {
        _logger = logger;
    }

    public async Task RunCommandAsync(string command)
    {
        Process? process;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            process = CreateLinuxShell(command);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process = CreateWindowsShell(command);
        }
        else
        {
            _logger.LogError("Platform not supported");
            throw new NotSupportedException();
        }

        if (!process.Start()) return;

        var reader = process.StandardOutput;
        while (!reader.EndOfStream)
        {
            var content = await reader.ReadLineAsync();

            if (content is not null)
            {
                _logger.LogInformation(content);
            }
        }

        await process.WaitForExitAsync();
    }

    private Process CreateLinuxShell(string command)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "sh",
            ArgumentList = {"-c", command},
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        return new Process {StartInfo = processStartInfo};
    }

    private Process CreateWindowsShell(string command)
    {
        var processStartInfo = new ProcessStartInfo
        {
            //TODO: customizable shell
            FileName = "pwsh",
            ArgumentList = {"-c", command},
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        return new Process {StartInfo = processStartInfo};
    }
}