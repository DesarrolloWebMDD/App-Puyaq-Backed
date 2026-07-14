using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Puyaq.Api.OpenApi;

/// <summary>
/// Personaliza la documentación OpenAPI de PUYAQ API.
/// </summary>
public sealed class PuyaqOpenApiDocumentTransformer
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Title = "PUYAQ API";
        document.Info.Version = "v1";
        document.Info.Description =
            "API oficial de PUYAQ para autenticación, usuarios y servicios de streaming.";

        return Task.CompletedTask;
    }
}