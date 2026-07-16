namespace Puyaq.Application.Authentication.DTOs
{
    /// <summary>
    /// Representa un proveedor de autenticación externo
    /// vinculado a la cuenta del usuario.
    /// </summary>
    public sealed class ExternalProvider
    {
        public string Provider { get; set; } =
            string.Empty;

        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool EmailVerified { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
