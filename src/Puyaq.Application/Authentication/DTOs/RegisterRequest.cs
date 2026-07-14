namespace Puyaq.Application.Authentication.DTOs;

/// <summary>
/// Contiene los datos necesarios para registrar una nueva cuenta en PUYAQ.
/// </summary>
/// <param name="Email">
/// Correo electrónico del usuario. Debe ser único y será utilizado para iniciar sesión.
/// </param>
/// <param name="Password">
/// Contraseña de acceso del usuario.
/// </param>
/// <param name="DisplayName">
/// Nombre que se mostrará públicamente dentro de la aplicación.
/// </param>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName);