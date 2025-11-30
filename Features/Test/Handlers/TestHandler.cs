using MinimalAPI.Features.Test.Models;
using MinimalAPI.Shared;

namespace MinimalAPI.Features.Test.Handlers;

/// <summary>
/// Handler pour traiter les requêtes de test
/// </summary>
public static class TestHandler
{
    /// <summary>
    /// Traite une requête de test et retourne une réponse
    /// </summary>
    public static Result<TestResponse> Handle(TestRequest request, string version)
    {
        // Logique métier pure - pas d'effets de bord
        var domain = new TestDomain(
            Message: request.Message.ToUpperInvariant(),
            Version: version,
            CreatedAt: DateTime.UtcNow
        );

        var response = new TestResponse(
            Message: domain.Message,
            Version: domain.Version,
            ProcessedAt: domain.CreatedAt
        );

        return Result<TestResponse>.Success(response);
    }
}

