using MinimalAPI.Features.Test.Handlers;
using MinimalAPI.Features.Test.Models;
using MinimalAPI.Features.Test.Validators;
using MinimalAPI.Shared;
using System.Diagnostics;

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
        var group = app.MapGroup("/api/tests")
            .WithTags("Tests");

        group.MapGet("/hello", HandleHello)
            .WithName("GetTestsHello")
            .WithSummary("Endpoint de test Hello")
            .WithDescription("Retourne un message de test avec la version de l'API. Paramètre optionnel: ?version=v1")
            .Produces<TestResponse>(200)
            .Produces<ErrorResponse>(400);

        group.MapPost("/", HandleProcess)
            .WithName("PostTests")
            .WithSummary("Création d'une requête de test")
            .WithDescription("Traite une requête de test avec validation et retourne une réponse. Paramètre optionnel: ?version=v1")
            .Accepts<TestRequest>("application/json")
            .Produces<TestResponse>(200)
            .Produces<ErrorResponse>(400);
    }

    private static string GetDefaultVersion(IConfiguration configuration)
    {
        return configuration["ApiSettings:DefaultVersion"] ?? "v1";
    }

    private static Task<IResult> HandleHello(
        string? version,
        IConfiguration configuration)
    {
        var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var apiVersion = string.IsNullOrEmpty(version)
            ? GetDefaultVersion(configuration)
            : version;

        return ErrorHandlingExtensions.HandleAsync(async () =>
        {
            var request = new TestRequest("Hello World");
            var result = TestHandler.Handle(request, apiVersion);
            return await Task.FromResult(result);
        }, traceId);
    }

    private static Task<IResult> HandleProcess(
        TestRequest request,
        string? version,
        IConfiguration configuration)
    {
        var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var apiVersion = string.IsNullOrEmpty(version)
            ? GetDefaultVersion(configuration)
            : version;

        // Validation
        var validationResult = TestValidator.Validate(request);
        if (!validationResult.IsSuccess)
        {
            var errorResponse = ErrorResponse.ValidationError(validationResult.Error!, traceId: traceId);
            return Task.FromResult(Results.BadRequest(errorResponse) as IResult);
        }

        // Traitement avec gestion d'erreurs structurée
        return ErrorHandlingExtensions.HandleAsync(async () =>
        {
            var result = TestHandler.Handle(validationResult.Value!, apiVersion);
            return await Task.FromResult(result);
        }, traceId);
    }
}

