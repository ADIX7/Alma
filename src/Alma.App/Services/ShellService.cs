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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "sh",
                ArgumentList = {"-c", command},
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var process = Process.Start(processStartInfo);
            if (process is null) return;

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
            return;
        }

        _logger.LogError("Platform not supported");

        throw new NotSupportedException();
    }
}