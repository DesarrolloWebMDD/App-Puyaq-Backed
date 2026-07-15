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
    IClock clock)
    : IAuthenticationService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRegister(request);

        var email = request.Email.Trim();
        var normalizedEmail = User.NormalizeEmail(email);

        var existingUser =
            await repository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (existingUser is not null)
        {
            throw new AppException(
                "AUTH_EMAIL_ALREADY_EXISTS",
                "Ya existe una cuenta registrada con el correo indicado.",
                StatusCodes.Status409Conflict);
        }

        var now = clock.UtcNow;

        var registeredUser =
            await repository.RegisterAsync(
                new RegisterUserCommand(
                    Id: Guid.NewGuid(),
                    Email: email,
                    NormalizedEmail: normalizedEmail,
                    DisplayName: request.DisplayName.Trim(),
                    Status: (int)UserStatus.Active,
                    PasswordHash: passwordService.Hash(
                        request.Password),
                    CreatedAt: now),
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

        var normalizedEmail =
            User.NormalizeEmail(request.Email);

        var user =
            await repository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordService.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new AppException(
                "AUTH_INVALID_CREDENTIALS",
                "El correo electrónico o la contraseña son incorrectos.",
                StatusCodes.Status401Unauthorized);
        }

        ValidateActiveUser(user);

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

        var device =
            AuthenticationMapper.ToDomain(request.Device);

        var identity =
            await externalIdentityProvider.ValidateAsync(
                request.Provider,
                request.Credential,
                cancellationToken);

        var now = clock.UtcNow;

        var user =
            await repository.GetByExternalLoginAsync(
                identity.Provider,
                identity.ProviderUserId,
                cancellationToken);

        /*
         * La identidad externa ya está vinculada.
         * Se actualizan únicamente los datos técnicos del proveedor.
         */
        if (user is not null)
        {
            ValidateActiveUser(user);

            await repository.UpdateLastLoginAsync(
                user.Id,
                now,
                cancellationToken);

            await repository.UpdateExternalLoginAsync(
                identity.Provider,
                identity.ProviderUserId,
                identity.Email,
                identity.DisplayName,
                identity.ProfileImageUrl,
                identity.EmailVerified,
                now,
                now,
                cancellationToken);

            return await CreateAuthenticatedSessionAsync(
                user,
                device,
                cancellationToken);
        }

        /*
         * Para crear una nueva cuenta social necesitamos un correo
         * utilizable entregado y validado por el proveedor.
         */
        if (string.IsNullOrWhiteSpace(identity.Email))
        {
            throw new AppException(
                "AUTH_EXTERNAL_EMAIL_REQUIRED",
                "El proveedor no entregó un correo electrónico utilizable.",
                StatusCodes.Status422UnprocessableEntity);
        }

        var normalizedEmail =
            User.NormalizeEmail(identity.Email);

        var existingUser =
            await repository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        /*
         * No se vincula automáticamente por coincidencia de correo.
         * El usuario deberá autenticarse con su método actual y usar
         * el endpoint protegido de vinculación.
         */
        if (existingUser is not null)
        {
            throw new AppException(
                "AUTH_EXTERNAL_LINK_REQUIRED",
                "Ya existe una cuenta PUYAQ con este correo. " +
                "Inicia sesión con tu método actual y vincula " +
                "el proveedor desde seguridad.",
                StatusCodes.Status409Conflict);
        }

        user =
            await repository.RegisterExternalUserAsync(
                new RegisterExternalUserCommand(
                    UserId: Guid.NewGuid(),
                    ExternalLoginId: Guid.NewGuid(),
                    Email: identity.Email.Trim(),
                    NormalizedEmail: normalizedEmail,
                    DisplayName:
                        string.IsNullOrWhiteSpace(
                            identity.DisplayName)
                            ? identity.Email.Trim()
                            : identity.DisplayName.Trim(),
                    Status: (int)UserStatus.Active,
                    Provider: identity.Provider,
                    ProviderUserId:
                        identity.ProviderUserId,
                    ProfileImageUrl:
                        identity.ProfileImageUrl,
                    EmailVerified:
                        identity.EmailVerified,
                    CreatedAt: now),
                cancellationToken);

        return await CreateAuthenticatedSessionAsync(
            user,
            device,
            cancellationToken);
    }

    public async Task LinkExternalProviderAsync(
        Guid userId,
        LinkExternalProviderRequest request,
        CancellationToken cancellationToken)
    {
        ValidateLinkExternalProvider(request);

        var provider =
            NormalizeProvider(request.Provider);

        var currentUser =
            await repository.GetByIdAsync(
                userId,
                cancellationToken)
            ?? throw new AppException(
                "AUTH_USER_NOT_FOUND",
                "No se encontró la cuenta autenticada.",
                StatusCodes.Status404NotFound);

        ValidateActiveUser(currentUser);

        var identity =
            await externalIdentityProvider.ValidateAsync(
                provider,
                request.Credential,
                cancellationToken);

        /*
         * La clave real de una identidad externa es:
         * Provider + ProviderUserId.
         */
        var externalUser =
            await repository.GetByExternalLoginAsync(
                identity.Provider,
                identity.ProviderUserId,
                cancellationToken);

        if (externalUser is not null)
        {
            if (externalUser.Id == userId)
            {
                throw new AppException(
                    "AUTH_PROVIDER_ALREADY_LINKED",
                    "El proveedor ya está vinculado a tu cuenta.",
                    StatusCodes.Status409Conflict);
            }

            throw new AppException(
                "AUTH_PROVIDER_LINKED_TO_ANOTHER_ACCOUNT",
                "La identidad externa ya está vinculada " +
                "a otra cuenta PUYAQ.",
                StatusCodes.Status409Conflict);
        }

        if (string.IsNullOrWhiteSpace(identity.Email))
        {
            throw new AppException(
                "AUTH_EXTERNAL_EMAIL_REQUIRED",
                "El proveedor no devolvió un correo utilizable.",
                StatusCodes.Status422UnprocessableEntity);
        }

        /*
         * Para vinculación segura, el correo validado por el
         * proveedor debe coincidir con el correo de la cuenta.
         */
        var normalizedExternalEmail =
            User.NormalizeEmail(identity.Email);

        if (!normalizedExternalEmail.Equals(
                currentUser.NormalizedEmail,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(
                "AUTH_EXTERNAL_EMAIL_MISMATCH",
                "El correo del proveedor no corresponde " +
                "a la cuenta PUYAQ autenticada.",
                StatusCodes.Status409Conflict);
        }

        var now = clock.UtcNow;

        await repository.LinkExternalProviderAsync(
            new LinkExternalProviderCommand(
                Id: Guid.NewGuid(),
                UserId: userId,
                Provider: identity.Provider,
                ProviderUserId:
                    identity.ProviderUserId,
                Email: identity.Email,
                DisplayName: identity.DisplayName,
                ProfileImageUrl:
                    identity.ProfileImageUrl,
                EmailVerified:
                    identity.EmailVerified,
                CreatedAt: now),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<DTOs.ExternalProvider>>
        GetExternalProvidersAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        var providers =
            await repository.GetExternalProvidersAsync(
                userId,
                cancellationToken);

        return providers
            .Select(
                AuthenticationMapper.ToExternalProvider)
            .ToList();
    }

    public async Task UnlinkExternalProviderAsync(
        Guid userId,
        string provider,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            provider);

        var normalizedProvider =
            NormalizeProvider(provider);

        var providers =
            await repository.GetExternalProvidersAsync(
                userId,
                cancellationToken);

        var providerExists =
            providers.Any(item =>
                item.Provider.Equals(
                    normalizedProvider,
                    StringComparison.OrdinalIgnoreCase));

        if (!providerExists)
        {
            throw new AppException(
                "AUTH_PROVIDER_NOT_LINKED",
                "El proveedor no está vinculado a la cuenta.",
                StatusCodes.Status404NotFound);
        }

        /*
         * El conteo considera:
         * - contraseña configurada;
         * - cada proveedor externo vinculado.
         */
        var methodsCount =
            await repository.CountAuthenticationMethodsAsync(
                userId,
                cancellationToken);

        if (methodsCount <= 1)
        {
            throw new AppException(
                "AUTH_LAST_METHOD_CANNOT_BE_REMOVED",
                "No puedes eliminar tu último método de acceso.",
                StatusCodes.Status409Conflict);
        }

        await repository.UnlinkExternalProviderAsync(
            userId,
            normalizedProvider,
            cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        AuthenticationUserRecord user,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var token =
            tokenService.CreateTokenPair(user);

        await SaveAuthenticatedSessionAsync(
            user,
            token,
            now,
            cancellationToken);

        return ToAuthResponse(
            user,
            token);
    }

    private async Task<AuthResponse>
        CreateAuthenticatedSessionAsync(
            AuthenticationUserRecord user,
            AuthenticationDevice device,
            CancellationToken cancellationToken)
    {
        /*
         * El dispositivo ya está convertido a Domain.
         * Posteriormente sus datos se persistirán en UserSessions.
         */
        _ = device;

        var now = clock.UtcNow;

        var token =
            tokenService.CreateTokenPair(user);

        await SaveAuthenticatedSessionAsync(
            user,
            token,
            now,
            cancellationToken);

        return ToAuthResponse(
            user,
            token);
    }

    private async Task SaveAuthenticatedSessionAsync(
        AuthenticationUserRecord user,
        TokenPair token,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await repository.SaveRefreshTokenAsync(
            new SaveRefreshTokenCommand(
                SessionId: Guid.NewGuid(),
                RefreshTokenId: Guid.NewGuid(),
                UserId: user.Id,
                TokenFamilyId: Guid.NewGuid(),
                TokenHash:
                    token.RefreshTokenHash,
                SessionExpiresAt:
                    token.RefreshTokenExpiresAt,
                TokenExpiresAt:
                    token.RefreshTokenExpiresAt,
                CreatedAt: now),
            cancellationToken);
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

    private static void ValidateRegister(
        RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Email);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Password);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.DisplayName);

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

    private static void ValidateLogin(
        LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Email);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Password);
    }

    private static void ValidateSocialLogin(
        SocialLoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Provider);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Credential);

        ArgumentNullException.ThrowIfNull(
            request.Device);

        NormalizeProvider(request.Provider);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Device.InstallationId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Device.Platform);
    }

    private static void ValidateLinkExternalProvider(
        LinkExternalProviderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Provider);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Credential);

        NormalizeProvider(request.Provider);
    }

    private static string NormalizeProvider(
        string provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            provider);

        var normalizedProvider =
            provider.Trim().ToUpperInvariant();

        if (normalizedProvider is not (
            "GOOGLE" or
            "FACEBOOK" or
            "APPLE"))
        {
            throw new AppException(
                "AUTH_EXTERNAL_PROVIDER_NOT_SUPPORTED",
                "El proveedor externo no está soportado.",
                StatusCodes.Status400BadRequest);
        }

        return normalizedProvider;
    }

    private static void ValidateActiveUser(
        AuthenticationUserRecord user)
    {
        if (user.Status != (int)UserStatus.Active)
        {
            throw new AppException(
                "AUTH_USER_NOT_ACTIVE",
                "La cuenta de usuario no se encuentra activa.",
                StatusCodes.Status403Forbidden);
        }
    }
}