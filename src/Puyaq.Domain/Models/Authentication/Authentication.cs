namespace Puyaq.Domain.Models.Authentication
{
    public sealed record LinkExternalProviderCommand(
     Guid Id,
     Guid UserId,
     string Provider,
     string ProviderUserId,
     string? Email,
     string? DisplayName,
     string? ProfileImageUrl,
     bool EmailVerified,
     DateTimeOffset CreatedAt);
}
