using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class Device : Entity
{
    private Device()
    {
    }

    public Guid UserId { get; private set; }
    public string InstallationId { get; private set; } = string.Empty;
    public string Platform { get; private set; } = string.Empty;
    public string? DeviceName { get; private set; }
    public string? AppVersion { get; private set; }
    public User User { get; private set; } = null!;
}
