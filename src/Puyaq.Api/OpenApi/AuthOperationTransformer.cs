using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Puyaq.Api.OpenApi;

/// <summary>
/// Agrega el requisito JWT Bearer únicamente
/// a los endpoints protegidos con Authorize.
/// </summary>
public sealed class AuthOperationTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata =
            context.Description.ActionDescriptor.EndpointMetadata;

        var allowsAnonymous = endpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (allowsAnonymous)
        {
            return Task.CompletedTask;
        }

        var requiresAuthorization = endpointMetadata
            .OfType<AuthorizeAttribute>()
            .Any();

        if (!requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}