using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Puyaq.CrossCutting.Settings;
using Puyaq.Infrastructure.Authentication;
using Puyaq.Infrastructure.Implementacion;
using Puyaq.Infrastructure.Interface;
using System.Text;

namespace Puyaq.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AppSettings>>(
                (options, appSettingsOptions) =>
                {
                    var jwt = appSettingsOptions.Value.Jwt;

                    ValidateJwtSettings(jwt);

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwt.Issuer,
                            ValidAudience = jwt.Audience,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwt.Key)),
                            ClockSkew = TimeSpan.FromSeconds(30)
                        };
                });

        services.AddAuthorization();

        services.AddScoped<GoogleIdentityValidator>();
        services.AddScoped<AppleIdentityValidator>();

        services.AddHttpClient<FacebookIdentityValidator>(
            client =>
            {
                client.BaseAddress =
                    new Uri("https://graph.facebook.com/");
            });

        services.AddScoped<IConnectionBase, ConnectionBase>();

        services.AddSingleton<
            Puyaq.CrossCutting.Time.IClock,
            Puyaq.CrossCutting.Time.SystemClock>();

        return services;
    }

    private static void ValidateJwtSettings(
        JwtSettings jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt.Issuer))
        {
            throw new InvalidOperationException(
                "No existe AppSettings:Jwt:Issuer.");
        }

        if (string.IsNullOrWhiteSpace(jwt.Audience))
        {
            throw new InvalidOperationException(
                "No existe AppSettings:Jwt:Audience.");
        }

        if (string.IsNullOrWhiteSpace(jwt.Key))
        {
            throw new InvalidOperationException(
                "No existe AppSettings:Jwt:Key.");
        }

        if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
        {
            throw new InvalidOperationException(
                "AppSettings:Jwt:Key debe tener al menos 32 bytes.");
        }

        if (jwt.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException(
                "AppSettings:Jwt:AccessTokenMinutes debe ser mayor que cero.");
        }

        if (jwt.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException(
                "AppSettings:Jwt:RefreshTokenDays debe ser mayor que cero.");
        }
    }
}