using Puyaq.Domain.Common;

namespace Puyaq.Domain.Auditing;

public sealed class AuditEvent : Entity
{
    private AuditEvent()
    {
    }

    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? MetadataJson { get; private set; }
}
