using System;
using System.Collections.Generic;
using System.Text;

namespace Puyaq.Domain.Models.Authentication
{
    public sealed record RegisterExternalUserCommand(
     Guid UserId,
     Guid ExternalLoginId,
     string Email,
     string NormalizedEmail,
     string DisplayName,
     int Status,
     string Provider,
     string ProviderUserId,
     string? ProfileImageUrl,
     bool EmailVerified,
     DateTimeOffset CreatedAt);
}
