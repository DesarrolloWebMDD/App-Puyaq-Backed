using Puyaq.Application.Authentication.DTOs;
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
}
