using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
    }

    public Guid SessionId { get; private set; }
    public Guid TokenFamilyId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public UserSession Session { get; private set; } = null!;
}
