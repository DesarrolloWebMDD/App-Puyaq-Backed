using Puyaq.Api.Exceptions;
using Puyaq.Api.OpenApi;
using Puyaq.Application;
using Puyaq.CrossCutting.Settings;
using Puyaq.Infrastructure;
using Puyaq.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AppSettings>()
    .Bind(
        builder.Configuration.GetSection(
            AppSettings.SectionName))
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddRepositories();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<
        PuyaqOpenApiDocumentTransformer>();

    options.AddSchemaTransformer<
        PuyaqOpenApiSchemaTransformer>();

    options.AddDocumentTransformer<
        BearerSecuritySchemeTransformer>();

    options.AddOperationTransformer<
        AuthOperationTransformer>();
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PuyaqMobile", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("PuyaqMobile");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "PUYAQ API v1");

        options.RoutePrefix = "swagger";
        options.DocumentTitle =
            "PUYAQ API - Documentación";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.EnablePersistAuthorization();
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;