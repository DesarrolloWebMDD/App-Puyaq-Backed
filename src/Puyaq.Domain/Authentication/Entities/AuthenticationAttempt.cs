using Puyaq.Domain.Common;

namespace Puyaq.Domain.Authentication.Entities;

public sealed class AuthenticationAttempt : Entity
{
    private AuthenticationAttempt()
    {
    }

    public string Identifier { get; private set; } = string.Empty;
    public bool Succeeded { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? FailureCode { get; private set; }
}
