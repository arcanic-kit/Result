namespace Arcanic.Result;

/// <summary>
/// Asynchronous counterparts of <see cref="ResultExtensions"/>.
/// </summary>
/// <remarks>
/// Naming convention:
/// <list type="bullet">
///   <item>The same name (<c>Map</c>, <c>Bind</c>, <c>Match</c>, ...) on a <see cref="Task{TResult}"/> of a result
///   takes a synchronous function, so pipelines can continue straight after an awaited call.</item>
///   <item>The <c>...Async</c> name takes an asynchronous function, on both a result and a task of a result.</item>
/// </list>
/// As with the synchronous combinators, functions only run on success and failures are passed along with all their errors.
/// </remarks>
public static class ResultAsyncExtensions
{
    // ---- Async functions on a result ------------------------------------

    /// <summary>Transforms the value of a successful result with an asynchronous function.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> map)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);

        return result.IsSuccess
            ? Result.Success(await map(result.Value).ConfigureAwait(false))
            : Result.Failure<TOut>(result.Errors);
    }

    /// <summary>Chains an asynchronous operation that can fail, passing it the value of a successful result.</summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result.Failure<TOut>(result.Errors);
    }

    /// <summary>Chains an asynchronous operation without a value that can fail, passing it the value of a successful result.</summary>
    public static async Task<Result> BindAsync<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result.Failure(result.Errors);
    }

    /// <summary>Chains an asynchronous operation without a value that can fail after a successful untyped result.</summary>
    public static async Task<Result> BindAsync(this Result result, Func<Task<Result>> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? await bind().ConfigureAwait(false)
            : Result.Failure(result.Errors);
    }

    /// <summary>Turns a successful result into a failure when its value does not satisfy an asynchronous condition.</summary>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Result<TValue> result, Func<TValue, Task<bool>> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (result.IsFailure || await predicate(result.Value).ConfigureAwait(false))
        {
            return result;
        }

        return Result.Failure<TValue>(error);
    }

    /// <summary>Runs an asynchronous side effect with the value of a successful result.</summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, Task> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            await action(result.Value).ConfigureAwait(false);
        }

        return result;
    }

    // ---- Sync functions on a task of a result ---------------------------

    /// <summary>Awaits the result, then transforms its value on success.</summary>
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> map) =>
        (await Await(resultTask)).Map(map);

    /// <summary>Awaits the result, then chains an operation that can fail.</summary>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> bind) =>
        (await Await(resultTask)).Bind(bind);

    /// <summary>Awaits the result, then chains an operation without a value that can fail.</summary>
    public static async Task<Result> Bind<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Result> bind) =>
        (await Await(resultTask)).Bind(bind);

    /// <summary>Awaits the untyped result, then chains an operation without a value that can fail.</summary>
    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Result> bind) =>
        (await Await(resultTask)).Bind(bind);

    /// <summary>Awaits the result, then fails with <paramref name="error"/> when the value does not satisfy the condition.</summary>
    public static async Task<Result<TValue>> Ensure<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, bool> predicate, Error error) =>
        (await Await(resultTask)).Ensure(predicate, error);

    /// <summary>Awaits the result, then runs a side effect with its value on success.</summary>
    public static async Task<Result<TValue>> Tap<TValue>(this Task<Result<TValue>> resultTask, Action<TValue> action) =>
        (await Await(resultTask)).Tap(action);

    /// <summary>Awaits the result, then runs a side effect on success.</summary>
    public static async Task<TResult> Tap<TResult>(this Task<TResult> resultTask, Action action)
        where TResult : Result =>
        (await Await(resultTask)).Tap(action);

    /// <summary>Awaits the result, then runs a side effect with every error on failure.</summary>
    public static async Task<TResult> TapError<TResult>(this Task<TResult> resultTask, Action<IReadOnlyList<Error>> action)
        where TResult : Result =>
        (await Await(resultTask)).TapError(action);

    /// <summary>Awaits the result, then executes the function matching its outcome.</summary>
    public static async Task<TOut> Match<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        (await Await(resultTask)).Match(onSuccess, onFailure);

    /// <summary>Awaits the untyped result, then executes the function matching its outcome.</summary>
    public static async Task<TOut> Match<TOut>(this Task<Result> resultTask, Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        (await Await(resultTask)).Match(onSuccess, onFailure);

    // ---- Async functions on a task of a result --------------------------

    /// <summary>Awaits the result, then transforms its value with an asynchronous function.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> map) =>
        await (await Await(resultTask)).MapAsync(map).ConfigureAwait(false);

    /// <summary>Awaits the result, then chains an asynchronous operation that can fail.</summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> bind) =>
        await (await Await(resultTask)).BindAsync(bind).ConfigureAwait(false);

    /// <summary>Awaits the result, then chains an asynchronous operation without a value that can fail.</summary>
    public static async Task<Result> BindAsync<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result>> bind) =>
        await (await Await(resultTask)).BindAsync(bind).ConfigureAwait(false);

    /// <summary>Awaits the untyped result, then chains an asynchronous operation without a value that can fail.</summary>
    public static async Task<Result> BindAsync(this Task<Result> resultTask, Func<Task<Result>> bind) =>
        await (await Await(resultTask)).BindAsync(bind).ConfigureAwait(false);

    /// <summary>Awaits the result, then fails with <paramref name="error"/> when the value does not satisfy the asynchronous condition.</summary>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, Task<bool>> predicate, Error error) =>
        await (await Await(resultTask)).EnsureAsync(predicate, error).ConfigureAwait(false);

    /// <summary>Awaits the result, then runs an asynchronous side effect with its value on success.</summary>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Task<Result<TValue>> resultTask, Func<TValue, Task> action) =>
        await (await Await(resultTask)).TapAsync(action).ConfigureAwait(false);

    private static System.Runtime.CompilerServices.ConfiguredTaskAwaitable<TResult> Await<TResult>(Task<TResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return task.ConfigureAwait(false);
    }
}
