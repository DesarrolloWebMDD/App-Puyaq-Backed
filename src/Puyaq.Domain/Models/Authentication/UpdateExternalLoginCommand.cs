using System;
using System.Collections.Generic;
using System.Text;

namespace Puyaq.Domain.Models.Authentication
{
    public sealed record UpdateExternalLoginCommand(
    string Provider,
    string ProviderUserId,
    string? Email,
    string? DisplayName,
    string? ProfileImageUrl,
    bool EmailVerified,
    DateTimeOffset UpdatedAt,
    DateTimeOffset LastLoginAt);
}
