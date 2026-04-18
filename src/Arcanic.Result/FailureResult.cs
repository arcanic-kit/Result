namespace Arcanic.Result;

/// <summary>
/// Represents a failed result that can be implicitly converted to either <see cref="Result"/> or <see cref="Result{TValue}"/>
/// without specifying the value type.
/// </summary>
[DebuggerDisplay("Failure: {Error.Code}")]
public readonly struct FailureResult
{
    private readonly Result _result;

    internal FailureResult(Error error)
    {
        _result = new Result(false, error);
    }

    /// <summary>Gets a value indicating whether the result is successful.</summary>
    public bool IsSuccess => _result.IsSuccess;

    /// <summary>Gets a value indicating whether the result is a failure.</summary>
    public bool IsFailure => _result.IsFailure;

    /// <summary>Gets the error.</summary>
    public Error Error => _result.Error;

    /// <summary>Matches the result and executes the appropriate function.</summary>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        _result.Match(onSuccess, onFailure);

    /// <summary>Matches the result and executes the appropriate action.</summary>
    public void Match(Action onSuccess, Action<Error> onFailure) =>
        _result.Match(onSuccess, onFailure);

    /// <summary>Implicitly converts to a non-generic failed result.</summary>
    public static implicit operator Result(FailureResult f) => f._result;
}
