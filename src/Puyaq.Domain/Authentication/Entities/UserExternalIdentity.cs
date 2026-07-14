using Puyaq.Domain.Authentication.Enums;
using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class UserExternalIdentity : Entity
{
    private UserExternalIdentity()
    {
    }

    public Guid UserId { get; private set; }
    public ExternalProvider Provider { get; private set; }
    public string ProviderUserId { get; private set; } = string.Empty;
    public string? ProviderEmail { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public User User { get; private set; } = null!;
}
