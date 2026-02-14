namespace Arcanic.Result;

/// <summary>
/// Extension methods for Result operations.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Ensures the condition is true, otherwise returns a failure result.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="condition">The condition function.</param>
    /// <param name="error">The error to return if the condition is false.</param>
    /// <returns>The original result if condition is true and result is successful, otherwise a failure result.</returns>
    public static Result Ensure(this Result result, Func<bool> condition, Error error) =>
        result.IsFailure ? result : Result.Ensure(condition(), error);

    /// <summary>
    /// Ensures the condition is true for the value, otherwise returns a failure result.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="condition">The condition function that takes the value.</param>
    /// <param name="error">The error to return if the condition is false.</param>
    /// <returns>The original result if condition is true and result is successful, otherwise a failure result.</returns>
    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> condition, Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return condition(result.Value) ? result : Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Combines multiple results into one. If any result is a failure, returns the first failure.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all results are successful, otherwise the first failure.</returns>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Combines multiple results into one. If any result is a failure, returns the first failure.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all results are successful, otherwise the first failure.</returns>
    public static Result Combine(IEnumerable<Result> results) => Combine(results.ToArray());

    /// <summary>
    /// Executes an action that returns void and wraps it in a Result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A successful result if no exception is thrown, otherwise a failure result.</returns>
    public static Result Try(Action action)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Exception", ex.Message));
        }
    }

    /// <summary>
    /// Executes a function and wraps it in a Result.
    /// </summary>
    /// <typeparam name="TValue">The return type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful result with the function result if no exception is thrown, otherwise a failure result.</returns>
    public static Result<TValue> Try<TValue>(Func<TValue> func)
    {
        try
        {
            return Result.Success(func());
        }
        catch (Exception ex)
        {
            return Result.Failure<TValue>(Error.Failure("Exception", ex.Message));
        }
    }

    /// <summary>
    /// Executes an async action and wraps it in a Result.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A successful result if no exception is thrown, otherwise a failure result.</returns>
    public static async Task<Result> TryAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Exception", ex.Message));
        }
    }

    /// <summary>
    /// Executes an async function and wraps it in a Result.
    /// </summary>
    /// <typeparam name="TValue">The return type.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A successful result with the function result if no exception is thrown, otherwise a failure result.</returns>
    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<TValue>> func)
    {
        try
        {
            var result = await func();
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<TValue>(Error.Failure("Exception", ex.Message));
        }
    }

    /// <summary>
    /// Binds an async function to a Result.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="func">The async function to bind.</param>
    /// <returns>The result of the async function if the original result was successful, otherwise the original result.</returns>
    public static async Task<Result> BindAsync(this Result result, Func<Task<Result>> func) =>
        result.IsSuccess ? await func() : result;

    /// <summary>
    /// Binds an async function to a Result{TValue}.
    /// </summary>
    /// <typeparam name="TValue">The input value type.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="func">The async function to bind.</param>
    /// <returns>The result of the async function if the original result was successful, otherwise the original result.</returns>
    public static async Task<Result> BindAsync<TValue>(this Result<TValue> result, Func<TValue, Task<Result>> func) =>
        result.IsSuccess ? await func(result.Value) : Result.Failure(result.Error);

    /// <summary>
    /// Binds an async function to a Result{TValue}.
    /// </summary>
    /// <typeparam name="TValue">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="func">The async function to bind.</param>
    /// <returns>The result of the async function if the original result was successful, otherwise a failure result.</returns>
    public static async Task<Result<TOut>> BindAsync<TValue, TOut>(this Result<TValue> result, Func<TValue, Task<Result<TOut>>> func) =>
        result.IsSuccess ? await func(result.Value) : Result.Failure<TOut>(result.Error);

    /// <summary>
    /// Maps an async function to a Result{TValue}.
    /// </summary>
    /// <typeparam name="TValue">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="func">The async function to map.</param>
    /// <returns>The mapped result if the original result was successful, otherwise a failure result.</returns>
    public static async Task<Result<TOut>> MapAsync<TValue, TOut>(this Result<TValue> result, Func<TValue, Task<TOut>> func) =>
        result.IsSuccess ? Result.Success(await func(result.Value)) : Result.Failure<TOut>(result.Error);
}