namespace Puyaq.Application.Authentication.DTOs
{
    /// <summary>
    /// Representa un proveedor de autenticación externo
    /// vinculado a la cuenta del usuario.
    /// </summary>
    public sealed class ExternalProvider
    {
        public string Provider { get; init; } = string.Empty;

        public string? Email { get; init; }

        public string? DisplayName { get; init; }

        public string? ProfileImageUrl { get; init; }

        public bool EmailVerified { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public DateTimeOffset? LastLoginAt { get; init; }
    }
}
