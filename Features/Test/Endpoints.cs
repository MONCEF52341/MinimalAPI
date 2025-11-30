using MinimalAPI.Features.Test.Handlers;
using MinimalAPI.Features.Test.Models;
using MinimalAPI.Features.Test.Validators;
using MinimalAPI.Shared;

namespace MinimalAPI.Features.Test;

/// <summary>
/// Définition des endpoints pour la feature Test
/// </summary>
public static class TestEndpoints
{
    /// <summary>
    /// Enregistre les endpoints de test dans l'application
    /// </summary>
    public static void MapTestEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var group = app.MapGroup("/api/test")
            .WithTags("Test");

        group.MapGet("/hello", HandleHello)
            .WithName("GetTestHello")
            .WithSummary("Endpoint de test Hello")
            .WithDescription("Retourne un message de test avec la version de l'API. Paramètre optionnel: ?version=v1")
            .Produces<TestResponse>(200)
            .Produces(400);

        group.MapPost("/process", HandleProcess)
            .WithName("PostTestProcess")
            .WithSummary("Traitement d'une requête de test")
            .WithDescription("Traite une requête de test avec validation et retourne une réponse. Paramètre optionnel: ?version=v1")
            .Accepts<TestRequest>("application/json")
            .Produces<TestResponse>(200)
            .Produces(400);
    }

    private static string GetDefaultVersion(IConfiguration configuration)
    {
        return configuration["ApiSettings:DefaultVersion"] ?? "v1";
    }

    private static Task<IResult> HandleHello(
        string? version,
        IConfiguration configuration)
    {
        var apiVersion = string.IsNullOrEmpty(version)
            ? GetDefaultVersion(configuration)
            : version;

        var request = new TestRequest("Hello World");
        var result = TestHandler.Handle(request, apiVersion);

        var response = result.Match(
            success => Results.Ok(success) as IResult,
            error => Results.BadRequest(new { error })
        );

        return Task.FromResult(response);
    }

    private static Task<IResult> HandleProcess(
        TestRequest request,
        string? version,
        IConfiguration configuration)
    {
        var apiVersion = string.IsNullOrEmpty(version)
            ? GetDefaultVersion(configuration)
            : version;

        // Validation
        var validationResult = TestValidator.Validate(request);
        if (!validationResult.IsSuccess)
        {
            return Task.FromResult(Results.BadRequest(new { error = validationResult.Error }) as IResult);
        }

        // Traitement
        var result = TestHandler.Handle(validationResult.Value!, apiVersion);

        var response = result.Match(
            success => Results.Ok(success) as IResult,
            error => Results.BadRequest(new { error })
        );

        return Task.FromResult(response);
    }
}

