using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Puyaq.CrossCutting.Exceptions;
using Puyaq.CrossCutting.Settings;
using Puyaq.Domain.Authentication.Entities;
using Puyaq.Infrastructure.Authentication.Models;
using System.Net.Http.Json;

namespace Puyaq.Infrastructure.Authentication;

public sealed class FacebookIdentityValidator(
    HttpClient httpClient,
    IOptions<AppSettings> options)
{
    private readonly SocialAuthenticationSettings _settings =
        options.Value.SocialAuthentication;

    public async Task<ExternalIdentity> ValidateAsync(
        string credential,
        CancellationToken cancellationToken = default)
    {
        var appAccessToken =
            $"{_settings.FacebookAppId}|{_settings.FacebookAppSecret}";

        var debugUrl =
            $"debug_token?input_token={Uri.EscapeDataString(credential)}" +
            $"&access_token={Uri.EscapeDataString(appAccessToken)}";

        var debugResponse =
            await httpClient.GetFromJsonAsync<FacebookDebugTokenResponse>(
                debugUrl,
                cancellationToken);

        var tokenData = debugResponse?.Data;

        if (tokenData is null ||
            !tokenData.IsValid ||
            tokenData.AppId != _settings.FacebookAppId ||
            string.IsNullOrWhiteSpace(tokenData.UserId))
        {
            throw new AppException(
                "AUTH_FACEBOOK_CREDENTIAL_INVALID",
                "La credencial de Facebook no es válida o expiró.",
                StatusCodes.Status401Unauthorized);
        }

        var userUrl =
            $"me?fields=id,name,email,picture" +
            $"&access_token={Uri.EscapeDataString(credential)}";

        var user =
            await httpClient.GetFromJsonAsync<FacebookUserResponse>(
                userUrl,
                cancellationToken)
            ?? throw new AppException(
                "AUTH_FACEBOOK_PROFILE_NOT_AVAILABLE",
                "No fue posible obtener el perfil de Facebook.",
                StatusCodes.Status401Unauthorized);

        return new ExternalIdentity
        {
            Provider = "FACEBOOK",
            ProviderUserId = user.Id,
            Email = user.Email,
            DisplayName = string.IsNullOrWhiteSpace(user.Name)
                ? user.Email
                : user.Name,
            ProfileImageUrl = user.Picture?.Data?.Url,
            EmailVerified = !string.IsNullOrWhiteSpace(user.Email)
        };
    }
}