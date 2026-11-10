# Arcanic.Result

A lightweight .NET library for explicit, type-safe error handling using the Result pattern — no exceptions for business logic failures.

Instead of throwing exceptions or returning `null`, methods return a `Result` that is either a success or a failure with a structured `Error`. The caller is then forced to handle both cases explicitly.

## Installation

```bash
dotnet add package Arcanic.Result
```

## Core concepts

### Result types

| Type | Description |
|---|---|
| `Result` | An operation that either succeeds or fails |
| `Result<T>` | An operation that either succeeds with a value or fails. Derives from `Result`, so any typed result can be handled as a `Result` |
| `Error` | A structured error with a code, description, and category |
| `ErrorType` | `Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden` |

### Creating results

```csharp
// Success
Result ok = Result.Success();
Result<User> user = Result.Success(new User(...));

// Failure
Result fail = Result.Failure(Error.Failure("DB.Timeout", "Database timed out"));
Result<User> notFound = Result.Failure<User>(Error.NotFound("User.NotFound", "User does not exist"));

// Implicit conversions — return a value or an error directly (the idiomatic form)
Result<int> count = 42;
Result<User> missing = Error.NotFound("User.NotFound", "User does not exist");
```

> C# does not apply user-defined conversions from interface types, so for results like
> `Result<IEnumerable<User>>` use `Result.Success(users)` instead of the implicit conversion.

### Error types

```csharp
Error.Failure("DB.Error",           "Unexpected database error");
Error.Validation("Email.Empty",     "Email is required");
Error.NotFound("User.NotFound",     "User does not exist");
Error.Conflict("Email.Taken",       "A user with this email already exists");
Error.Unauthorized("Auth.Required", "Authentication is required");
Error.Forbidden("Role.Missing",     "You do not have permission to perform this action");
```

### Handling results with Match

`Match` forces you to handle both outcomes. It comes in two flavors:

```csharp
// Return a value from each branch
IActionResult response = result.Match(
    onSuccess: user  => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound   => NotFound(error.Description),
        ErrorType.Validation => BadRequest(error.Description),
        _                    => Problem(error.Description)
    });

// Execute an action in each branch
result.Match(
    onSuccess: user  => Console.WriteLine($"Welcome, {user.Name}"),
    onFailure: error => Console.WriteLine($"Error: {error.Description}"));
```

`onFailure` receives the first error. When a result can fail for several reasons, read `result.Errors`.

### Multiple errors

A failure carries one or more errors. `Errors` lists all of them (empty on success); `Error` is the first.

```csharp
Result<User> invalid = Result.Failure<User>([
    Error.Validation("Email.Empty", "Email is required"),
    Error.Validation("Name.TooLong", "Name is too long"),
]);

invalid.Errors.Count; // 2
invalid.Error.Code;   // "Email.Empty"

// Combine runs every check and collects every error — typed and untyped results mix freely
Result validation = Result.Combine(ValidateEmail(email), ValidateName(name), FindTenant(tenantId));
```

### Chaining operations

The combinators run only on success; on failure they skip their function and pass the failure along with all of its errors.

| Method | Use it to |
|---|---|
| `Map(x => ...)` | transform the value |
| `Bind(x => ...)` | chain an operation that returns a `Result` / `Result<T>` |
| `Ensure(x => ..., error)` | fail with `error` when a condition does not hold |
| `Tap(x => ...)` | run a side effect on success (logging, metrics, ...) |
| `TapError(errors => ...)` | run a side effect with every error on failure |

```csharp
Result<string> greeting = ParseId(input)          // Result<int>
    .Bind(userRepository.Find)                    // Result<User>
    .Ensure(user => user.IsActive, UserErrors.Inactive)
    .Tap(user => logger.LogInformation("Greeting {User}", user.Id))
    .Map(user => $"Hello, {user.Name}");
```

### Async pipelines

On a `Task<Result<T>>`, the same names take synchronous functions; the `...Async` names (`MapAsync`, `BindAsync`, `EnsureAsync`, `TapAsync`) take asynchronous ones. `Match` also works on a task.

```csharp
string message = await userRepository.FindAsync(id)   // Task<Result<User>>
    .Ensure(user => user.IsActive, UserErrors.Inactive)
    .BindAsync(user => mailer.SendWelcomeAsync(user))  // Task<Result>
    .Match(() => "Welcome email sent", error => error.Description);
```

## Example — service and controller

```csharp
// Service — return values and errors directly via implicit conversion
public async Task<Result<User>> GetUserAsync(int id)
{
    if (id <= 0)
        return Error.Validation("User.InvalidId", "ID must be positive");

    var user = await _repository.GetByIdAsync(id);

    return user is not null
        ? user
        : Error.NotFound("User.NotFound", "User does not exist");
}

// Controller
[HttpGet("{id}")]
public Task<IActionResult> Get(int id) =>
    _userService.GetUserAsync(id).Match(
        onSuccess: user  => (IActionResult)Ok(user),
        onFailure: error => Problem(statusCode: error.Type switch
        {
            ErrorType.Validation   => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden    => 403,
            ErrorType.NotFound     => 404,
            ErrorType.Conflict     => 409,
            _                      => 500
        }, detail: error.Description));
```

See [`samples/CleanArchitecture`](samples/CleanArchitecture) for a complete Web API that maps every error into RFC 9457 problem details.

## Upgrading to v11

v11 makes `Result<T>` derive from `Result`. The [v11.0.0 release notes](.github/release-notes/v11.0.0.md) list the breaking changes and how to migrate; the most common one is replacing `Result.Failure(error)` in methods returning `Result<T>` with `return error;` or `Result.Failure<T>(error)`.

## License

MIT — see [LICENSE](LICENSE) for details.
