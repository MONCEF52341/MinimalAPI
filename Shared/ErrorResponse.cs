namespace MinimalAPI.Shared;

/// <summary>
/// Réponse d'erreur structurée pour éviter les erreurs 500 plain
/// </summary>
public record ErrorResponse
{
    public string ErrorCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? TraceId { get; init; }

    public static ErrorResponse Create(string errorCode, string message, string? details = null, string? traceId = null)
    {
        return new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Details = details,
            TraceId = traceId
        };
    }

    public static ErrorResponse ValidationError(string message, string? details = null, string? traceId = null)
    {
        return Create("VALIDATION_ERROR", message, details, traceId);
    }

    public static ErrorResponse NotFound(string resource, string? identifier = null)
    {
        var message = identifier != null
            ? $"{resource} with identifier '{identifier}' not found"
            : $"{resource} not found";
        return Create("NOT_FOUND", message);
    }

    public static ErrorResponse InternalError(string message, string? traceId = null)
    {
        return Create("INTERNAL_ERROR", message, null, traceId);
    }
}

