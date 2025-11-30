namespace MinimalAPI.Features.Test.Models;

/// <summary>
/// Modèle de requête pour l'endpoint de test
/// </summary>
public record TestRequest(string Message, string? OptionalField = null);

