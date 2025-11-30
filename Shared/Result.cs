namespace MinimalAPI.Shared;

/// <summary>
/// Pattern Result pour gérer les erreurs sans exceptions en programmation fonctionnelle
/// </summary>
public record Result<T>
{
  public bool IsSuccess { get; init; }
  public T? Value { get; init; }
  public string? Error { get; init; }

  private Result() { }

  public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
  public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };

  public TResult Match<TResult>(
      Func<T, TResult> onSuccess,
      Func<string, TResult> onFailure) =>
      IsSuccess ? onSuccess(Value!) : onFailure(Error!);

  public Result<TResult> Map<TResult>(Func<T, TResult> mapper) =>
      IsSuccess ? Result<TResult>.Success(mapper(Value!)) : Result<TResult>.Failure(Error!);

  public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) =>
      IsSuccess ? binder(Value!) : Result<TResult>.Failure(Error!);
}

