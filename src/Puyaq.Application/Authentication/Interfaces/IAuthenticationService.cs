using Puyaq.Application.Authentication.DTOs;
using Puyaq.Domain.Authentication.Enums;
using Puyaq.Repository.Interface;

namespace Puyaq.Application.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(
    RegisterRequest request,
    CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> SocialLoginAsync(
        SocialLoginRequest request,
        CancellationToken cancellationToken);

    Task LinkExternalProviderAsync(
        Guid userId,
        LinkExternalProviderRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DTOs.ExternalProvider>>
        GetExternalProvidersAsync(
            Guid userId,
            CancellationToken cancellationToken);

    Task UnlinkExternalProviderAsync(
        Guid userId,
        string provider,
        CancellationToken cancellationToken);


}
