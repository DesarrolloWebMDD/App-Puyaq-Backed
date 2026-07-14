namespace Puyaq.Application.Authentication.DTOs;

/// <summary>
/// Contiene las credenciales necesarias para iniciar sesión en PUYAQ.
/// </summary>
/// <param name="Email">
/// Correo electrónico asociado a la cuenta del usuario.
/// </param>
/// <param name="Password">
/// Contraseña de acceso del usuario.
/// </param>
public sealed record LoginRequest(
    string Email,
    string Password);