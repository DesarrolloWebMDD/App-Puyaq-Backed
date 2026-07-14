using Microsoft.AspNetCore.Http;

using Puyaq.CrossCutting.Exceptions;
using Puyaq.Domain.Authentication.Entities;

namespace Puyaq.Infrastructure.Authentication;

public sealed class AppleIdentityValidator
{
    public Task<ExternalIdentity> ValidateAsync(
        string credential,
        CancellationToken cancellationToken = default)
    {
        throw new AppException(
            "AUTH_APPLE_NOT_ENABLED",
            "La autenticación con Apple todavía no está habilitada.",
            StatusCodes.Status501NotImplemented);
    }
}