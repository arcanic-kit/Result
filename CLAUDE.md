# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore dependencies
dotnet restore Arcanic.Result.slnx

# Build
dotnet build Arcanic.Result.slnx --configuration Release --no-restore

# Run all tests
dotnet test Arcanic.Result.slnx --configuration Release --no-build --verbosity normal

# Run tests with coverage
dotnet test Arcanic.Result.slnx --configuration Release --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run a single test class
dotnet test tests/Arcanic.Result.Tests --filter "FullyQualifiedName~ErrorTests"

# Pack NuGet package
dotnet pack Arcanic.Result.slnx --configuration Release --no-build --output ./packages
```

## Architecture

This is a **Railway-Oriented Programming** library for .NET 8/9/10 that provides explicit success/failure result types. It avoids exceptions for business logic control flow.

### Core abstractions (`src/Arcanic.Result/`)

- **`ErrorType`** — enum classifying errors: `Failure`, `Validation`, `Conflict`, `NotFound`
- **`Error`** — sealed record with `Code`, `Description`, and `ErrorType`. Built via static factories: `Error.Failure()`, `Error.Validation()`, `Error.NotFound()`, `Error.Conflict()`. Special constants: `Error.None`, `Error.NullValue`
- **`Result`** — base class with `IsSuccess`/`IsFailure`/`Error`. Created via `Result.Success()` / `Result.Failure(error)`. Supports `Match<TOut>()` for pattern matching
- **`Result<TValue>`** — generic subclass adding a `Value` property (throws if accessed on failure). Supports implicit conversions from both `TValue` and `Error`

### Implicit conversion convention

```csharp
Result<string> r1 = "hello";        // success
Result<string> r2 = Error.NotFound("X.NotFound", "Not found");  // failure
```

### Error code naming convention

Dot notation for domain-scoped codes: `"Product.NotFound"`, `"User.EmailConflict"`. Generic codes use UPPER_SNAKE_CASE: `"INVALID_EMAIL"`.

### Clean Architecture sample (`samples/CleanArchitecture/`)

Demonstrates the intended usage pattern:
- **Application layer**: services return `Result<T>` — no exceptions thrown for business failures
- **Web API layer**: controllers switch on `error.Type` to map `ErrorType` → HTTP status codes

### Multi-targeting

The library targets .NET 8, 9, and 10 simultaneously (configured in `Directory.Build.props`). CI tests all three. Nullable reference types and implicit usings are enabled project-wide.
