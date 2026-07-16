namespace Puyaq.Domain.Authentication.Entities
{
    /// <summary>
    /// Representa una identidad externa vinculada
    /// a una cuenta PUYAQ.
    /// </summary>
    public sealed class ExternalLogin
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Provider { get; set; } =
            string.Empty;

        public string ProviderUserId { get; set; } =
            string.Empty;

        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool EmailVerified { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
