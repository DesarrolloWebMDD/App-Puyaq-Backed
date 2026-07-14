using Microsoft.AspNetCore.Http;
using Puyaq.CrossCutting.Exceptions;
using Puyaq.Domain.Authentication.Entities;
using Puyaq.Infrastructure.Interface;

namespace Puyaq.Infrastructure.Authentication;

public sealed class ExternalIdentityService(
    GoogleIdentityValidator googleValidator,
    FacebookIdentityValidator facebookValidator,
    AppleIdentityValidator appleValidator)
    : IExternalIdentityService
{
    public Task<ExternalIdentity> ValidateAsync(
        string provider,
        string credential,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(credential);

        var normalizedProvider =
            provider.Trim().ToUpperInvariant();

        return normalizedProvider switch
        {
            "GOOGLE" => googleValidator.ValidateAsync(
                credential,
                cancellationToken),

            "FACEBOOK" => facebookValidator.ValidateAsync(
                credential,
                cancellationToken),

            "APPLE" => appleValidator.ValidateAsync(
                credential,
                cancellationToken),

            _ => throw new AppException(
                "AUTH_EXTERNAL_PROVIDER_NOT_SUPPORTED",
                "El proveedor de autenticación no está soportado.",
                StatusCodes.Status400BadRequest)
        };
    }
}