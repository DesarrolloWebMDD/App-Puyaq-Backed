using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Puyaq.Api.OpenApi;

/// <summary>
/// Registra el esquema de autenticación JWT Bearer
/// dentro del documento OpenAPI de PUYAQ.
/// </summary>
public sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await authenticationSchemeProvider.GetAllSchemesAsync();

        var hasBearerScheme = authenticationSchemes.Any(
            scheme => scheme.Name == "Bearer");

        if (!hasBearerScheme)
        {
            return;
        }

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes =
            new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "Ingrese únicamente el access token JWT. " +
                        "Swagger agregará automáticamente el prefijo Bearer."
                }
            };
    }
}