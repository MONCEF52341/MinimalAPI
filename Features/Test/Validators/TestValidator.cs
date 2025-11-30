using MinimalAPI.Features.Test.Models;
using MinimalAPI.Shared;

namespace MinimalAPI.Features.Test.Validators;

/// <summary>
/// Validateur pour les requêtes de test
/// </summary>
public static class TestValidator
{
    public static Result<TestRequest> Validate(TestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Result<TestRequest>.Failure("Le message ne peut pas être vide");
        }

        if (request.Message.Length > 100)
        {
            return Result<TestRequest>.Failure("Le message ne peut pas dépasser 100 caractères");
        }

        return Result<TestRequest>.Success(request);
    }
}

