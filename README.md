# Arcanic Result

A lightweight Result pattern implementation for .NET that provides explicit error handling and type-safe operations without exceptions.

## Features

- **Type-safe error handling** - No more exceptions for business logic failures
- **Rich error types** - Support for different error categories (Validation, NotFound, Conflict, Failure)
- **Pattern matching** - Comprehensive Match methods for handling success and failure cases
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

### Basic Result Operations

```csharp
public Result<UserDto> GetUser(int userId)
{
    var userResult = ValidateUserId(userId);

    return userResult.Match(
        onSuccess: id => GetUserFromDatabase(id).Match(
            onSuccess: user => Result.Success(MapToUserDto(user)),
            onFailure: error => Result.Failure<UserDto>(error)
        ),
        onFailure: error => Result.Failure<UserDto>(error)
    );
}

private Result<int> ValidateUserId(int userId)
{
    return userId > 0 
        ? Result.Success(userId)
        : Result.Failure<int>(Error.Validation("INVALID_ID", "User ID must be positive"));
}

private Result<User> GetUserFromDatabase(int userId)
{
    var user = _repository.GetById(userId);
    return user is not null 
        ? Result.Success(user)
        : Result.Failure<User>(Error.NotFound("USER_NOT_FOUND", "User not found"));
}

private UserDto MapToUserDto(User user)
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
var result = GetUser(userId);

// Pattern matching for return values
var response = result.Match(
    onSuccess: user => Ok(user),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => NotFound(error.Description),
        ErrorType.Validation => BadRequest(error.Description),
        _ => Problem(error.Description)
    }
);

// Action-based matching for side effects
result.Match(
    onSuccess: user => Console.WriteLine($"User: {user.Name}"),
    onFailure: error => Console.WriteLine($"Error: {error.Description}")
);
```

### Exception Handling

```csharp
// Handle exceptions manually in your methods
private Result<User> ParseUserFromJson(string json)
{
    try
    {
        var user = JsonSerializer.Deserialize<User>(json);
        return user is not null 
            ? Result.Success(user)
            : Result.Failure<User>(Error.Failure("PARSE_ERROR", "Failed to parse user from JSON"));
    }
    catch (Exception ex)
    {
        return Result.Failure<User>(Error.Failure("JSON_ERROR", ex.Message));
    }
}

// Using the result
var parseResult = ParseUserFromJson(jsonString);
parseResult.Match(
    onSuccess: user => ProcessUser(user),
    onFailure: error => LogError(error)
);
```

## Advanced Examples

### Service Layer Implementation

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public Result<User> CreateUser(CreateUserRequest request)
    {
        var validationResult = ValidateRequest(request);

        return validationResult.Match(
            onSuccess: validRequest => CheckEmailNotExists(validRequest.Email).Match(
                onSuccess: _ => CreateUserInternal(validRequest).Match(
                    onSuccess: user => 
                    {
                        _emailService.SendWelcomeEmail(user.Email);
                        return Result.Success(user);
                    },
                    onFailure: error => Result.Failure<User>(error)
                ),
                onFailure: error => Result.Failure<User>(error)
            ),
            onFailure: error => Result.Failure<User>(error)
        );
    }

    private Result<CreateUserRequest> ValidateRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Error.Validation("INVALID_EMAIL", "Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Error.Validation("INVALID_PASSWORD", "Password is required");

        return Result.Success(request);
    }

    private Result CheckEmailNotExists(string email)
    {
        var existingUser = _repository.GetByEmail(email);
        return existingUser is null
            ? Result.Success()
            : Result.Failure(Error.Conflict("EMAIL_EXISTS", "User with this email already exists"));
    }

    private Result<User> CreateUserInternal(CreateUserRequest request)
    {
        try
        {
            var user = new User
            {
                Email = request.Email,
                Password = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _repository.Add(user);
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
    public IActionResult GetUser(int id)
    {
        var result = _userService.GetUser(id);

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
    public IActionResult CreateUser(CreateUserRequest request)
    {
        var result = _userService.CreateUser(request);

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
2. **Pattern Matching** - Handle success and failure cases with functional style pattern matching
3. **Type Safety** - Compile-time guarantees about error handling
4. **Performance** - No exception throwing for business logic failures
5. **Testability** - Easy to test both success and failure paths
6. **Readability** - Clear separation between success and failure flows using Match method

## License

MIT License - see LICENSE file for details