namespace Puyaq.Repository.Interface;

public interface ITokenService
{
    TokenPair CreateTokenPair(AuthenticationUserRecord user);
}

public sealed record TokenPair(
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash,
    int ExpiresIn,
    DateTimeOffset RefreshTokenExpiresAt);
