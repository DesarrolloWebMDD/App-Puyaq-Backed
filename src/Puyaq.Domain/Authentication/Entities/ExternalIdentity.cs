namespace Puyaq.Domain.Authentication.Entities;

/// <summary>
/// Representa una identidad externa validada por un proveedor social.
/// </summary>
public sealed class ExternalIdentity
{
    public string Provider { get; init; } = string.Empty;

    public string ProviderUserId { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string? DisplayName { get; init; }

    public string? ProfileImageUrl { get; init; }

    public bool EmailVerified { get; init; }
}