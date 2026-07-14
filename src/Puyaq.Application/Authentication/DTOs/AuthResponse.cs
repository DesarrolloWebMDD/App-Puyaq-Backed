namespace Puyaq.Application.Authentication.DTOs;

/// <summary>
/// Representa la respuesta generada después de una autenticación exitosa en PUYAQ.
/// </summary>
/// <param name="AccessToken">
/// Token de acceso utilizado para autenticar las solicitudes a endpoints protegidos.
/// </param>
/// <param name="RefreshToken">
/// Token utilizado para renovar la sesión y obtener un nuevo token de acceso.
/// </param>
/// <param name="ExpiresIn">
/// Tiempo de vigencia del token de acceso expresado en segundos.
/// </param>
/// <param name="User">
/// Información básica del usuario autenticado.
/// </param>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthenticatedUserDto User);

/// <summary>
/// Representa la información básica del usuario autenticado en PUYAQ.
/// </summary>
/// <param name="Id">
/// Identificador único del usuario.
/// </param>
/// <param name="Email">
/// Correo electrónico asociado a la cuenta del usuario.
/// </param>
/// <param name="DisplayName">
/// Nombre visible del usuario dentro de la aplicación.
/// </param>
public sealed record AuthenticatedUserDto(
    Guid Id,
    string Email,
    string DisplayName);