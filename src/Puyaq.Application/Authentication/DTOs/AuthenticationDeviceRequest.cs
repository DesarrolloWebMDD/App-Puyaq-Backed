
namespace Puyaq.Application.Authentication.DTOs
{
    public sealed record AuthenticationDeviceRequest(
     string InstallationId,
     string Platform,
     string? DeviceName,
     string? AppVersion);
}
