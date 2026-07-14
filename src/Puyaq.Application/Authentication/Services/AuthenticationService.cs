using Microsoft.AspNetCore.Http;
using Puyaq.Application.Authentication.DTOs;
using Puyaq.Application.Authentication.Interfaces;
using Puyaq.Application.Authentication.Mappers;
using Puyaq.CrossCutting.Exceptions;
using Puyaq.CrossCutting.Time;
using Puyaq.Domain.Authentication.Entities;
using Puyaq.Domain.Authentication.Enums;
using Puyaq.Domain.Models.Authentication;
using Puyaq.Repository.Interface;

namespace Puyaq.Application.Authentication.Services;

public sealed class AuthenticationService(
    IAuthenticationRepository repository,
    IPasswordService passwordService,
    ITokenService tokenService,
    IExternalIdentityProvider externalIdentityProvider,
    IClock clock) : IAuthenticationService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRegister(request);

        var email = request.Email.Trim();
        var normalizedEmail = User.NormalizeEmail(email);
        var existing = await repository.GetByNormalizedEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (existing is not null)
        {
            throw new AppException(
                "AUTH_EMAIL_ALREADY_EXISTS",
                "Ya existe una cuenta registrada con el correo indicado.",
                StatusCodes.Status409Conflict);
        }

        var now = clock.UtcNow;
        var registeredUser = await repository.RegisterAsync(
            new RegisterUserCommand(
                Guid.NewGuid(),
                email,
                normalizedEmail,
                request.DisplayName.Trim(),
                (int)UserStatus.Active,
                passwordService.Hash(request.Password),
                now),
            cancellationToken);

        return await CreateAuthResponseAsync(
            registeredUser,
            now,
            cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateLogin(request);

        var normalizedEmail = User.NormalizeEmail(request.Email);
        var user = await repository.GetByNormalizedEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordService.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException(
                "AUTH_INVALID_CREDENTIALS",
                "El correo electrónico o la contraseña son incorrectos.",
                StatusCodes.Status401Unauthorized);
        }

        if (user.Status != (int)UserStatus.Active)
        {
            throw new AppException(
                "AUTH_USER_NOT_ACTIVE",
                "La cuenta de usuario no se encuentra activa.",
                StatusCodes.Status403Forbidden);
        }

        var now = clock.UtcNow;
        await repository.UpdateLastLoginAsync(
            user.Id,
            now,
            cancellationToken);

        return await CreateAuthResponseAsync(
            user,
            now,
            cancellationToken);
    }

    public async Task<AuthResponse> SocialLoginAsync(
        SocialLoginRequest request,
        CancellationToken cancellationToken)
    {
        ValidateSocialLogin(request);

        var device = AuthenticationMapper.ToDomain(request.Device);
        var identity = await externalIdentityProvider.ValidateAsync(
            request.Provider,
            request.Credential,
            cancellationToken);

        var now = clock.UtcNow;
        var user = await repository.GetByExternalLoginAsync(
            identity.Provider,
            identity.ProviderUserId,
            cancellationToken);

        if (user is not null)
        {
            await repository.UpdateExternalLoginAsync(
                new UpdateExternalLoginCommand(
                    identity.Provider,
                    identity.ProviderUserId,
                    identity.Email,
                    identity.DisplayName,
                    identity.ProfileImageUrl,
                    identity.EmailVerified,
                    now,
                    now),
                cancellationToken);

            return await CreateAuthenticatedSessionAsync(
                user,
                device,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(identity.Email))
        {
            throw new AppException(
                "AUTH_EXTERNAL_EMAIL_REQUIRED",
                "El proveedor no entregó un correo electrónico utilizable.",
                StatusCodes.Status422UnprocessableEntity);
        }

        var normalizedEmail = User.NormalizeEmail(identity.Email);
        var existingUser = await repository.GetByNormalizedEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new AppException(
                "AUTH_EXTERNAL_LINK_REQUIRED",
                "Ya existe una cuenta PUYAQ con este correo. Inicia sesión con tu método actual y vincula el proveedor desde seguridad.",
                StatusCodes.Status409Conflict);
        }

        user = await repository.RegisterExternalUserAsync(
            new RegisterExternalUserCommand(
                UserId: Guid.NewGuid(),
                ExternalLoginId: Guid.NewGuid(),
                Email: identity.Email.Trim(),
                NormalizedEmail: normalizedEmail,
                DisplayName: string.IsNullOrWhiteSpace(identity.DisplayName)
                    ? identity.Email.Trim()
                    : identity.DisplayName.Trim(),
                Status: (int)UserStatus.Active,
                Provider: identity.Provider,
                ProviderUserId: identity.ProviderUserId,
                ProfileImageUrl: identity.ProfileImageUrl,
                EmailVerified: identity.EmailVerified,
                CreatedAt: now),
            cancellationToken);

        return await CreateAuthenticatedSessionAsync(
            user,
            device,
            cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        AuthenticationUserRecord user,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var token = tokenService.CreateTokenPair(user);

        await repository.SaveRefreshTokenAsync(
            new SaveRefreshTokenCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                user.Id,
                Guid.NewGuid(),
                token.RefreshTokenHash,
                token.RefreshTokenExpiresAt,
                token.RefreshTokenExpiresAt,
                now),
            cancellationToken);

        return ToAuthResponse(user, token);
    }

    private async Task<AuthResponse> CreateAuthenticatedSessionAsync(
        AuthenticationUserRecord user,
        AuthenticationDevice device,
        CancellationToken cancellationToken)
    {
        _ = device;
        var now = clock.UtcNow;
        var token = tokenService.CreateTokenPair(user);

        await repository.SaveRefreshTokenAsync(
            new SaveRefreshTokenCommand(
                SessionId: Guid.NewGuid(),
                RefreshTokenId: Guid.NewGuid(),
                UserId: user.Id,
                TokenFamilyId: Guid.NewGuid(),
                TokenHash: token.RefreshTokenHash,
                SessionExpiresAt: token.RefreshTokenExpiresAt,
                TokenExpiresAt: token.RefreshTokenExpiresAt,
                CreatedAt: now),
            cancellationToken);

        return ToAuthResponse(user, token);
    }

    private static AuthResponse ToAuthResponse(
        AuthenticationUserRecord user,
        TokenPair token)
    {
        return new AuthResponse(
            token.AccessToken,
            token.RefreshToken,
            token.ExpiresIn,
            new AuthenticatedUserDto(
                user.Id,
                user.Email,
                user.DisplayName));
    }

    private static void ValidateRegister(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DisplayName);

        if (!request.Email.Contains('@'))
        {
            throw new AppException(
                "AUTH_INVALID_EMAIL",
                "El correo electrónico no tiene un formato válido.",
                StatusCodes.Status422UnprocessableEntity);
        }

        if (request.Password.Length < 8)
        {
            throw new AppException(
                "AUTH_PASSWORD_POLICY_FAILED",
                "La contraseña debe tener al menos 8 caracteres.",
                StatusCodes.Status422UnprocessableEntity);
        }
    }

    private static void ValidateLogin(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
    }

    private static void ValidateSocialLogin(SocialLoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Credential);
        ArgumentNullException.ThrowIfNull(request.Device);

        var provider = request.Provider.Trim().ToUpperInvariant();

        if (provider is not ("GOOGLE" or "FACEBOOK" or "APPLE"))
        {
            throw new AppException(
                "AUTH_EXTERNAL_PROVIDER_NOT_SUPPORTED",
                "El proveedor social indicado no está soportado.",
                StatusCodes.Status400BadRequest);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Device.InstallationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Device.Platform);
    }
}
