using System.Runtime.InteropServices;

namespace Alma.Services;

public class OsInformation : IOsInformation
{
    private const string OsIdentifierDefault = "default";
    private const string OsIdentifierWin = "windows";
    private const string OsIdentifierMac = "macos";
    private const string OsIdentifierFreeBsd = "freebsd";
    private const string OsIdentifierLinux = "linux";

    private const string LinuxOsRelease = "/etc/os-release";

    public async Task<string> GetOsIdentifierAsync()
    {
        string? baseOsIdentifier = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) baseOsIdentifier = OsIdentifierWin;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) baseOsIdentifier = OsIdentifierMac;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) baseOsIdentifier = OsIdentifierFreeBsd;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            baseOsIdentifier = OsIdentifierLinux;

            try
            {
                if (File.Exists(LinuxOsRelease))
                {
                    var lines = await File.ReadAllLinesAsync(LinuxOsRelease);
                    var distroName = lines.FirstOrDefault(l => l.StartsWith("id=", StringComparison.InvariantCultureIgnoreCase));
                    if (distroName is not null)
                    {
                        distroName = distroName.ToLower().Substring(distroName.IndexOf("=", StringComparison.Ordinal) + 1);

                        baseOsIdentifier += "-" + distroName;
                    }
                }
            }
            catch
            {
            }
        }

        if (baseOsIdentifier is null)
            return "unknown";

        var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLower();

        return baseOsIdentifier + "-" + architecture;
    }

    public async Task<bool> IsOnPlatformAsync(string platform)
    {
        return platform == OsIdentifierDefault
               || (await GetOsIdentifierAsync()).StartsWith(platform, StringComparison.InvariantCultureIgnoreCase);
    }
}