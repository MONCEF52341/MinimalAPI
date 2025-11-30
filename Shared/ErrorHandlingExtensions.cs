using System.Diagnostics;

namespace MinimalAPI.Shared;

/// <summary>
/// Extensions pour la gestion d'erreurs structurées
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Convertit un Result en IResult avec gestion d'erreurs structurée
    /// </summary>
    public static IResult ToResult<T>(this Result<T> result, string? traceId = null)
    {
        return result.Match(
            success => Results.Ok(success),
            error => Results.BadRequest(ErrorResponse.ValidationError(error, traceId: traceId))
        );
    }

    /// <summary>
    /// Wrapper pour gérer les exceptions et retourner des erreurs structurées
    /// </summary>
    public static async Task<IResult> HandleAsync<T>(
        Func<Task<Result<T>>> operation,
        string? traceId = null)
    {
        try
        {
            var result = await operation();
            return result.ToResult(traceId);
        }
        catch (Exception ex)
        {
            var errorId = traceId ?? Activity.Current?.Id ?? Guid.NewGuid().ToString();
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "An error occurred",
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = errorId,
                    ["errorCode"] = "INTERNAL_ERROR"
                }
            );
        }
    }

    /// <summary>
    /// Wrapper synchrone pour gérer les exceptions
    /// </summary>
    public static IResult Handle<T>(Func<Result<T>> operation, string? traceId = null)
    {
        try
        {
            var result = operation();
            return result.ToResult(traceId);
        }
        catch (Exception ex)
        {
            var errorId = traceId ?? Activity.Current?.Id ?? Guid.NewGuid().ToString();
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "An error occurred",
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = errorId,
                    ["errorCode"] = "INTERNAL_ERROR"
                }
            );
        }
    }
}

