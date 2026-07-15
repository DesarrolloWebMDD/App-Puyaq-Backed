using Puyaq.Application.Authentication.DTOs;
using Puyaq.Domain.Authentication.Entities;

namespace Puyaq.Application.Authentication.Mappers;

public static class AuthenticationMapper
{
    public static AuthenticationDevice ToDomain(
        AuthenticationDeviceRequest request)
    {
        return new AuthenticationDevice
        {
            InstallationId = request.InstallationId.Trim(),
            Platform = request.Platform.Trim().ToUpperInvariant(),
            DeviceName = request.DeviceName?.Trim(),
            AppVersion = request.AppVersion?.Trim()
        };
    }

  
   public static ExternalProviderDto ToDto(
            ExternalLogin externalLogin)
    {
      return new ExternalProviderDto(
          externalLogin.Provider,
          externalLogin.Email,
          externalLogin.DisplayName,
          externalLogin.ProfileImageUrl,
          externalLogin.EmailVerified,
          externalLogin.CreatedAt,
          externalLogin.LastLoginAt);
    }
    public static ExternalProvider ToExternalProvider(
     ExternalLogin externalLogin)
    {
        return new ExternalProvider
        {
            Provider = externalLogin.Provider,
            Email = externalLogin.Email,
            DisplayName = externalLogin.DisplayName,
            ProfileImageUrl = externalLogin.ProfileImageUrl,
            EmailVerified = externalLogin.EmailVerified,
            CreatedAt = externalLogin.CreatedAt,
            LastLoginAt = externalLogin.LastLoginAt
        };
    }
}