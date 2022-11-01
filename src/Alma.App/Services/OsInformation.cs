using System.Runtime.InteropServices;

namespace Alma.Services;

public class OsInformation : IOsInformation
{
    private const string OsIdentifierDefault = "default";
    private const string OsIdentifierWin = "windows";
    private const string OsIdentifierLinux = "linux";

    public string GetOsIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OsIdentifierWin;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OsIdentifierLinux;

        return "unknown";
    }

    public bool IsOnPlatform(string platform)
    {
        if (platform == OsIdentifierDefault) return true;
        return platform == GetOsIdentifier();
    }
}