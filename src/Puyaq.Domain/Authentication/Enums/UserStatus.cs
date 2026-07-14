namespace Puyaq.Domain.Authentication.Enums;

public enum UserStatus
{
    PendingEmailVerification = 1,
    Active = 2,
    TemporarilyLocked = 3,
    Suspended = 4,
    DeletionPending = 5,
    Deleted = 6
}
