using Puyaq.Domain.Authentication.Enums;
using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class User : Entity
{
    private User()
    {
    }

    private User(string email, string displayName)
    {
        Email = email;
        NormalizedEmail = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        Status = UserStatus.Active;
    }

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    public UserCredential? Credential { get; private set; }
    public ICollection<UserExternalIdentity> ExternalIdentities { get; private set; } = [];
    public ICollection<UserSession> Sessions { get; private set; } = [];

    public static User Create(string email, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new User(email.Trim(), displayName);
    }

    public void SetCredential(UserCredential credential) =>
        Credential = credential ?? throw new ArgumentNullException(nameof(credential));

    public void MarkLogin(DateTimeOffset utcNow)
    {
        LastLoginAt = utcNow;
        UpdatedAt = utcNow;
    }

    public static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
