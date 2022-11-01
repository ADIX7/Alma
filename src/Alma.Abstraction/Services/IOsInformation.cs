namespace Alma.Services;

public interface IOsInformation
{
    string GetOsIdentifier();
    bool IsOnPlatform(string platform);
}