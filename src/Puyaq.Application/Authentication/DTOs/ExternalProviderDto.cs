namespace Puyaq.Application.Authentication.DTOs
{
    /// <summary>
    /// Representa un proveedor externo vinculado
    /// a una cuenta PUYAQ.
    /// </summary>
    public sealed record ExternalProviderDto(
       string Provider,
       string? Email,
       string? DisplayName,
       string? ProfileImageUrl,
       bool EmailVerified,
       DateTimeOffset CreatedAt,
       DateTimeOffset? LastLoginAt);
}
