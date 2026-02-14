# Arcanic.Result

A robust Result pattern implementation for .NET applications inspired by functional programming principles and to error handling.

## Features

- **Type-safe error handling** - No more exceptions for business logic failures
- **Railway-oriented programming** - Chain operations with automatic error propagation
- **Rich error types** - Support for different error categories (Validation, NotFound, Conflict, Failure)
- **Async support** - Full async/await compatibility
- **Fluent API** - Expressive and readable code
- **Implicit conversions** - Seamless integration with existing code

## Installation

```bash
dotnet add package Arcanic.Result
```

## Basic Usage

### Creating Results

```csharp
using Arcanic.Result;

// Success results
var success = Result.Success();
var successWithValue = Result.Success("Hello World");

// Failure results
var error = Error.Failure("USER_NOT_FOUND", "User with specified ID was not found");
var failure = Result.Failure(error);
var failureWithType = Result.Failure<string>(error);

// Using implicit conversions
Result<string> result = "success value"; // Implicit success
Result<string> result2 = error; // Implicit failure
```

### Error Types

```csharp
// Different error types for different scenarios
var validationError = Error.Validation("INVALID_EMAIL", "Email format is invalid");
var notFoundError = Error.NotFound("USER_NOT_FOUND", "User not found");
var conflictError = Error.Conflict("EMAIL_EXISTS", "User with this email already exists");
var failureError = Error.Failure("DATABASE_ERROR", "Failed to connect to database");
```

### Chaining Operations (Railway Pattern)

```csharp
public async Task<Result<UserDto>> GetUserAsync(int userId)
{
    return await Result.Success(userId)
        .Bind(ValidateUserId)
        .BindAsync(GetUserFromDatabaseAsync)
        .MapAsync(MapToUserDto)
        .Ensure(user => user.IsActive, Error.Validation("USER_INACTIVE", "User is not active"));
}

private Result<int> ValidateUserId(int userId)
{
    return userId > 0 
        ? Result.Success(userId)
        : Result.Failure<int>(Error.Validation("INVALID_ID", "User ID must be positive"));
}

private async Task<Result<User>> GetUserFromDatabaseAsync(int userId)
{
    var user = await _repository.GetByIdAsync(userId);
    return user is not null 
        ? Result.Success(user)
        : Result.Failure<User>(Error.NotFound("USER_NOT_FOUND", "User not found"));
}

private async Task<UserDto> MapToUserDto(User user)
{
    return new UserDto 
    { 
        Id = user.Id, 
        Name = user.Name, 
        Email = user.Email 
    };
}
```

### Error Handling with Match

```csharp
var result = await GetUserAsync(userId);

// Pattern matching
var response = result.Match(
    onSuccess: user => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => NotFound(error.Description),
        ErrorType.Validation => BadRequest(error.Description),
        _ => Problem(error.Description)
    }
);

// Action-based matching
result.Match(
    onSuccess: user => Console.WriteLine($"User: {user.Name}"),
    onFailure: error => Console.WriteLine($"Error: {error.Description}")
);
```

### Exception Handling

```csharp
// Wrap potentially throwing operations
var result = ResultExtensions.Try(() => JsonSerializer.Deserialize<User>(json));

// Async version
var asyncResult = await ResultExtensions.TryAsync(async () => 
    await httpClient.GetStringAsync("https://api.example.com/users"));
```

### Combining Multiple Results

```csharp
var validation1 = ValidateEmail(email);
var validation2 = ValidatePassword(password);
var validation3 = ValidateAge(age);

var combinedResult = ResultExtensions.Combine(validation1, validation2, validation3);

if (combinedResult.IsSuccess)
{
    // All validations passed
    await CreateUserAsync(email, password, age);
}
```

### Extension Methods

```csharp
var result = await GetUserAsync(userId)
    .Ensure(user => user.IsActive, Error.Validation("INACTIVE", "User is inactive"))
    .Ensure(user => user.IsVerified, Error.Validation("UNVERIFIED", "User is not verified"))
    .MapAsync(async user => await EnrichUserDataAsync(user));
```

## Advanced Examples

### Service Layer Implementation

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        return await ValidateRequest(request)
            .BindAsync(async _ => await CheckEmailNotExists(request.Email))
            .BindAsync(async _ => await CreateUser(request))
            .TapAsync(async user => await _emailService.SendWelcomeEmailAsync(user.Email));
    }

    private Result<CreateUserRequest> ValidateRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Error.Validation("INVALID_EMAIL", "Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Error.Validation("INVALID_PASSWORD", "Password is required");

        return Result.Success(request);
    }

    private async Task<Result> CheckEmailNotExists(string email)
    {
        var existingUser = await _repository.GetByEmailAsync(email);
        return existingUser is null
            ? Result.Success()
            : Result.Failure(Error.Conflict("EMAIL_EXISTS", "User with this email already exists"));
    }

    private async Task<Result<User>> CreateUser(CreateUserRequest request)
    {
        try
        {
            var user = new User
            {
                Email = request.Email,
                Password = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(user);
            return Result.Success(user);
        }
        catch (Exception ex)
        {
            return Result.Failure<User>(Error.Failure("DATABASE_ERROR", ex.Message));
        }
    }
}
```

### API Controller Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserAsync(id);

        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: error => error.Type switch
            {
                ErrorType.NotFound => NotFound(new { message = error.Description }),
                ErrorType.Validation => BadRequest(new { message = error.Description }),
                _ => Problem(detail: error.Description, title: error.Code)
            }
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);

        return result.Match(
            onSuccess: user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
            onFailure: error => error.Type switch
            {
                ErrorType.Validation => BadRequest(new { message = error.Description }),
                ErrorType.Conflict => Conflict(new { message = error.Description }),
                _ => Problem(detail: error.Description, title: error.Code)
            }
        );
    }
}
```

## Benefits

1. **Explicit Error Handling** - All failure cases are explicit in the method signature
2. **Composability** - Chain operations without nested try-catch blocks
3. **Type Safety** - Compile-time guarantees about error handling
4. **Performance** - No exception throwing for business logic failures
5. **Testability** - Easy to test both success and failure paths
6. **Readability** - Clear separation between success and failure flows

## Inspiration

This library is inspired by:
- Milan Jovanovic's approach to Result patterns in .NET
- Functional programming principles from F# and other functional languages
- Railway-oriented programming concepts

## License

MIT License - see LICENSE file for details