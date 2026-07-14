using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class EmailVerificationToken : Entity
{
    private EmailVerificationToken()
    {
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }
}
