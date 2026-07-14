using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Puyaq.Application.Authentication.DTOs;

namespace Puyaq.Api.OpenApi;

/// <summary>
/// Personaliza los esquemas OpenAPI utilizados por PUYAQ API.
/// </summary>
public sealed class PuyaqOpenApiSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(RegisterRequest))
        {
            schema.Example = new JsonObject
            {
                ["email"] = "david@puyaq.com",
                ["password"] = "Puyaq2026*",
                ["displayName"] = "David"
            };
        }

        if (context.JsonTypeInfo.Type == typeof(LoginRequest))
        {
            schema.Example = new JsonObject
            {
                ["email"] = "david@puyaq.com",
                ["password"] = "Puyaq2026*"
            };
        }

        return Task.CompletedTask;
    }
}