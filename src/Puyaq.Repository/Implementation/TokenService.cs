using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Puyaq.CrossCutting.Settings;
using Puyaq.Repository.Interface;

namespace Puyaq.Repository.Implementation;

public sealed class TokenService(IOptions<AppSettings> options) : ITokenService
{
    private readonly JwtSettings _settings = options.Value.Jwt;

    public TokenPair CreateTokenPair(AuthenticationUserRecord user)
    {
        ValidateSettings();

        var now = DateTimeOffset.UtcNow;
        var accessExpires = now.AddMinutes(_settings.AccessTokenMinutes);
        var refreshExpires = now.AddDays(_settings.RefreshTokenDays);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim("display_name", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessExpires.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        return new TokenPair(
            accessToken,
            refreshToken,
            refreshTokenHash,
            _settings.AccessTokenMinutes * 60,
            refreshExpires);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Issuer) ||
            string.IsNullOrWhiteSpace(_settings.Audience) ||
            string.IsNullOrWhiteSpace(_settings.Key) ||
            Encoding.UTF8.GetByteCount(_settings.Key) < 32 ||
            _settings.AccessTokenMinutes <= 0 ||
            _settings.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("La configuración AppSettings:Jwt no es válida.");
        }
    }
}
