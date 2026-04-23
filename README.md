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
| `Result<T>` | An operation that either succeeds with a value or fails |
| `Error` | A structured error with a code, description, and category |
| `ErrorType` | `Failure`, `Validation`, `NotFound`, `Conflict` |

### Creating results

```csharp
// Success
Result ok = Result.Success();
Result<User> user = Result.Success(new User(...));

// Failure — Result.Failure implicitly converts to Result<T>
Result fail = Result.Failure(Error.Failure("DB.Timeout", "Database timed out"));
Result<User> notFound = Result.Failure(Error.NotFound("User.NotFound", "User does not exist"));
```

### Error types

```csharp
Error.Failure("DB.Error",       "Unexpected database error");
Error.Validation("Email.Empty", "Email is required");
Error.NotFound("User.NotFound", "User does not exist");
Error.Conflict("Email.Taken",   "A user with this email already exists");
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
            ErrorType.NotFound   => NotFound(error.Description),
            ErrorType.Validation => BadRequest(error.Description),
            _                    => Problem(error.Description)
        });
```

## License

MIT — see [LICENSE](LICENSE) for details.
