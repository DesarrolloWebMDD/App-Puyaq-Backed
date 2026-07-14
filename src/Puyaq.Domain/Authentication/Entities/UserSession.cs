using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class UserSession : Entity
{
    private UserSession()
    {
    }

    public Guid UserId { get; private set; }
    public Guid? DeviceId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }
    public User User { get; private set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
}
