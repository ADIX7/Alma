namespace Alma.Services;

public interface IOsInformation
{
    Task<string> GetOsIdentifierAsync();
    Task<bool> IsOnPlatformAsync(string platform);
}