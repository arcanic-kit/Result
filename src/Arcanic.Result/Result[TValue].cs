namespace Arcanic.Result;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
[DebuggerDisplay("{IsSuccess ? \"Success: \" + Value : \"Failure: \" + Error.Code}")]
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access the value of a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    internal static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    internal static new Result<TValue> Failure(Error error) => new(default, false, error);

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">The function to execute on success with the value.</param>
    /// <param name="onFailure">The function to execute on failure with the error.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    /// Matches the result and executes the appropriate action.
    /// </summary>
    /// <param name="onSuccess">The action to execute on success with the value.</param>
    /// <param name="onFailure">The action to execute on failure with the error.</param>
    public void Match(Action<TValue> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value);
        }
        else
        {
            onFailure(Error);
        }
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <remarks>
    /// C# does not apply user-defined conversions from interface types, so when <typeparamref name="TValue"/>
    /// is an interface (e.g. <c>IEnumerable&lt;T&gt;</c>) use <see cref="Result.Success{TValue}(TValue)"/> instead.
    /// </remarks>
    /// <param name="value">The value.</param>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a typed failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
