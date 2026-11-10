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

## Example — service and controller

```csharp
// Service
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return Result.Failure(Error.Validation("User.InvalidId", "ID must be positive"));

    var user = _repository.GetById(id);

    return user is not null
        ? Result.Success(user)
        : Result.Failure(Error.NotFound("User.NotFound", "User does not exist"));
}

// Controller
[HttpGet("{id}")]
public IActionResult Get(int id) =>
    _userService.GetUser(id).Match(
        onSuccess: user  => Ok(user),
        onFailure: error => error.Type switch
        {
            ErrorType.NotFound    => NotFound(error.Description),
            ErrorType.Validation  => BadRequest(error.Description),
            ErrorType.Unauthorized => Unauthorized(),
            ErrorType.Forbidden   => Forbid(),
            _                     => Problem(error.Description)
        });
```

## License

MIT — see [LICENSE](LICENSE) for details.
