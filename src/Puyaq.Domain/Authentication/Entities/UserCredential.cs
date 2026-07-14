using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class UserCredential : Entity
{
    private UserCredential()
    {
    }

    private UserCredential(Guid userId, string passwordHash)
    {
        UserId = userId;
        PasswordHash = passwordHash;
    }

    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public User User { get; private set; } = null!;

    public static UserCredential Create(Guid userId, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        return new UserCredential(userId, passwordHash);
    }

    public void ChangePassword(string passwordHash, DateTimeOffset utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        UpdatedAt = utcNow;
    }
}
