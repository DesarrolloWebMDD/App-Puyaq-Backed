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
}