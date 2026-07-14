using System;
using System.Collections.Generic;
using System.Text;

namespace Puyaq.Domain.Models.Authentication
{
    public sealed record ExternalLoginRecord(
     Guid ExternalLoginId,
     Guid UserId,
     string Provider,
     string ProviderUserId,
     string? Email,
     string? DisplayName,
     string? ProfileImageUrl,
     bool EmailVerified);
}
