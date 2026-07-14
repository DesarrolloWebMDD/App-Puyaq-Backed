using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Puyaq.CrossCutting.Settings;
using Puyaq.Domain.Authentication.Entities;

namespace Puyaq.Infrastructure.Authentication;

public sealed class GoogleIdentityValidator
{
    private readonly SocialAuthenticationSettings _settings;

    public GoogleIdentityValidator(
        IOptions<AppSettings> options)
    {
        _settings = options.Value.SocialAuthentication;
    }

    public async Task<ExternalIdentity> ValidateAsync(
        string credential,
        CancellationToken cancellationToken = default)
    {
        var validationSettings =
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience =
                [
                    _settings.GoogleClientId
                ]
            };

        var payload =
            await GoogleJsonWebSignature.ValidateAsync(
                credential,
                validationSettings);

        return new ExternalIdentity
        {
            Provider = "GOOGLE",
            ProviderUserId = payload.Subject,
            Email = payload.Email,
            DisplayName = string.IsNullOrWhiteSpace(payload.Name)
                ? payload.Email
                : payload.Name,
            ProfileImageUrl = payload.Picture,
            EmailVerified = payload.EmailVerified
        };
    }
}