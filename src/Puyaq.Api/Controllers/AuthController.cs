using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Puyaq.Application.Authentication.DTOs;
using Puyaq.Application.Authentication.Interfaces;
using Puyaq.CrossCutting.Results;

namespace Puyaq.Api.Controllers;

/// <summary>
/// Gestiona el registro y la autenticación de los usuarios de PUYAQ.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController(
    IAuthenticationService authenticationService) : ControllerBase
{
    /// <summary>
    /// Registra una nueva cuenta de usuario.
    /// </summary>
    /// <remarks>
    /// Crea una cuenta en PUYAQ utilizando los datos proporcionados.
    ///
    /// El correo electrónico debe ser único. Al finalizar correctamente,
    /// devuelve la información del usuario y los datos de autenticación.
    /// </remarks>
    /// <param name="request">
    /// Datos necesarios para registrar una nueva cuenta en PUYAQ.
    /// </param>
    /// <returns>
    /// Información de autenticación del usuario registrado.
    /// </returns>
    /// <response code="201">
    /// La cuenta fue creada correctamente.
    /// </response>
    /// <response code="400">
    /// Los datos enviados no son válidos.
    /// </response>
    /// <response code="409">
    /// Ya existe una cuenta registrada con el correo indicado.
    /// </response>
    /// <response code="500">
    /// Ocurrió un error interno al procesar la solicitud.
    /// </response>
    [HttpPost("register")]
    [ProducesResponseType(
        typeof(ApiResponse<AuthResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiError),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiError),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApiError),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.RegisterAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(
                result,
                "Cuenta creada correctamente.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Inicia sesión en PUYAQ.
    /// </summary>
    /// <remarks>
    /// Valida el correo electrónico y la contraseña proporcionados.
    ///
    /// Cuando las credenciales son correctas, devuelve la información
    /// del usuario y el token de acceso correspondiente.
    /// </remarks>
    /// <param name="request">
    /// Credenciales necesarias para iniciar sesión.
    /// </param>
    /// <param name="cancellationToken">
    /// Token para cancelar la operación.
    /// </param>
    /// <returns>
    /// Información de autenticación del usuario.
    /// </returns>
    /// <response code="200">
    /// La sesión fue iniciada correctamente.
    /// </response>
    /// <response code="400">
    /// Los datos enviados no son válidos.
    /// </response>
    /// <response code="401">
    /// El correo electrónico o la contraseña son incorrectos.
    /// </response>
    /// <response code="500">
    /// Ocurrió un error interno al procesar la solicitud.
    /// </response>
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(ApiResponse<AuthResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiError),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiError),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ApiError),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(
            request,
            cancellationToken);

        return Ok(
            ApiResponse<AuthResponse>.Ok(
                result,
                "Sesión iniciada correctamente.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Registra o inicia sesión mediante un proveedor externo.
    /// </summary>
    /// <remarks>
    /// Valida la credencial directamente con Google, Facebook o Apple.
    /// Luego crea una sesión propia de PUYAQ.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("social")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> SocialLogin(
     [FromBody] SocialLoginRequest request,
     CancellationToken cancellationToken)
    {
        var result =
            await authenticationService.SocialLoginAsync(
                request,
                cancellationToken);

        return Ok(
            ApiResponse<AuthResponse>.Ok(
                result,
                "Autenticación social realizada correctamente.",
                HttpContext.TraceIdentifier));
    }

}