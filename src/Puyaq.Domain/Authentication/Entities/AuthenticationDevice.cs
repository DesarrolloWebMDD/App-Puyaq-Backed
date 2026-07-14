namespace Puyaq.Domain.Authentication.Entities;

/// <summary>
/// Representa el dispositivo desde el cual se inicia una sesión.
/// </summary>
public sealed class AuthenticationDevice
{
    public string InstallationId { get; init; } = string.Empty;

    public string Platform { get; init; } = string.Empty;

    public string? DeviceName { get; init; }

    public string? AppVersion { get; init; }
}