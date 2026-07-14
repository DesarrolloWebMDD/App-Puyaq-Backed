namespace Puyaq.CrossCutting.Settings;

/// <summary>
/// Representa la configuración general de PUYAQ.
/// </summary>
public sealed class AppSettings
{
    public const string SectionName = "AppSettings";

    public ConnectionStringSettings ConnectionStrings { get; init; } = new();

    public JwtSettings Jwt { get; init; } = new();
    public SocialAuthenticationSettings SocialAuthentication { get; set; }
    = new();
}

public sealed class ConnectionStringSettings
{
    public string DefaultConnection { get; init; } = string.Empty;
}

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; }

    public int RefreshTokenDays { get; init; }
}

