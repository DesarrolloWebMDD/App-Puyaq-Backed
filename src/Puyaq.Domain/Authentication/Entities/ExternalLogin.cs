namespace Puyaq.Domain.Authentication.Entities
{
    /// <summary>
    /// Representa una identidad externa vinculada
    /// a una cuenta PUYAQ.
    /// </summary>
    public sealed class ExternalLogin
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Provider { get; init; } = string.Empty;
        public string ProviderUserId { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? DisplayName { get; init; }
        public string? ProfileImageUrl { get; init; }
        public bool EmailVerified { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }
    }
}
