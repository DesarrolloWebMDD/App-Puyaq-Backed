using System.Text.Json.Serialization;

namespace Puyaq.Infrastructure.Authentication.Models;

internal sealed class FacebookDebugTokenResponse
{
    [JsonPropertyName("data")]
    public FacebookDebugTokenData Data { get; init; } = new();
}

internal sealed class FacebookDebugTokenData
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; init; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; init; }

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; init; }
}

internal sealed class FacebookUserResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("picture")]
    public FacebookPicture? Picture { get; init; }
}

internal sealed class FacebookPicture
{
    [JsonPropertyName("data")]
    public FacebookPictureData? Data { get; init; }
}

internal sealed class FacebookPictureData
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}