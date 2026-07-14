
namespace Puyaq.Application.Authentication.DTOs
{
    public sealed record SocialLoginRequest(
     string Provider,
     string Credential,
     AuthenticationDeviceRequest Device);
}
