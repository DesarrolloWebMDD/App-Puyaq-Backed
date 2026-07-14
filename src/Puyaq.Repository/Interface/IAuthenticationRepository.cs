using Puyaq.Domain.Models.Authentication;

namespace Puyaq.Repository.Interface;

public interface IAuthenticationRepository
{
    Task<AuthenticationUserRecord?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task<AuthenticationUserRecord> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(
        Guid userId,
        DateTimeOffset lastLoginAt,
        CancellationToken cancellationToken = default);

    Task SaveRefreshTokenAsync(
        SaveRefreshTokenCommand command,
        CancellationToken cancellationToken = default);


    Task<AuthenticationUserRecord?> GetByExternalLoginAsync(
      string provider,
      string providerUserId,
      CancellationToken cancellationToken = default);



    Task<AuthenticationUserRecord> RegisterExternalUserAsync(
        RegisterExternalUserCommand command,
        CancellationToken cancellationToken = default);

    Task UpdateExternalLoginAsync(
        UpdateExternalLoginCommand command,
        CancellationToken cancellationToken = default);




}

public sealed record AuthenticationUserRecord(
    Guid Id,
    string Email,
    string NormalizedEmail,
    string DisplayName,
    int Status,
    string? PasswordHash);

public sealed record RegisterUserCommand(
    Guid Id,
    string Email,
    string NormalizedEmail,
    string DisplayName,
    int Status,
    string PasswordHash,
    DateTimeOffset CreatedAt);

public sealed record SaveRefreshTokenCommand(
    Guid SessionId,
    Guid RefreshTokenId,
    Guid UserId,
    Guid TokenFamilyId,
    string TokenHash,
    DateTimeOffset SessionExpiresAt,
    DateTimeOffset TokenExpiresAt,
    DateTimeOffset CreatedAt);
