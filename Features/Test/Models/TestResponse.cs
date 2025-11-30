namespace MinimalAPI.Features.Test.Models;

/// <summary>
/// Modèle de réponse pour l'endpoint de test
/// </summary>
public record TestResponse(string Message, string Version, DateTime ProcessedAt);

