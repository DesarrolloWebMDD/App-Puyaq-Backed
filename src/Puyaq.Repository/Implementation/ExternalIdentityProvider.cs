using Puyaq.Domain.Authentication.Entities;
using Puyaq.Infrastructure.Authentication;
using Puyaq.Repository.Interface;

namespace Puyaq.Repository.Implementation;

public sealed class ExternalIdentityProvider(
    GoogleIdentityValidator googleValidator,
    FacebookIdentityValidator facebookValidator,
    AppleIdentityValidator appleValidator)
    : IExternalIdentityProvider
{
    public Task<ExternalIdentity> ValidateAsync(
        string provider,
        string credential,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(credential);

        return provider.Trim().ToUpperInvariant() switch
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

            _ => throw new InvalidOperationException(
                "El proveedor social no está soportado.")
        };
    }
}
