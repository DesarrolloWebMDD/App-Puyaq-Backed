using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Puyaq.CrossCutting.Results;

namespace Puyaq.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
   
    [HttpGet("info")]
    public ActionResult<ApiResponse<object>> Info()
    {
        var data = new
        {
            Name = "PUYAQ API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        };

        return Ok(
            ApiResponse<object>.Ok(
                data,
                traceId: HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpGet("secure")]
    [ProducesResponseType(
      typeof(ApiResponse<object>),
      StatusCodes.Status200OK)]
    [ProducesResponseType(
      StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> Secure()
    {
        var data = new
        {
            UserId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,

            Email = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email)?.Value,

            DisplayName = User.FindFirst(
                System.Security.Claims.ClaimTypes.Name)?.Value
        };

        return Ok(
            ApiResponse<object>.Ok(
                data,
                "Token válido.",
                HttpContext.TraceIdentifier));
    }
}