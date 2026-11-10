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

- **`ErrorType`** — enum classifying errors: `Failure`, `Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden`
- **`Error`** — sealed record with `Code`, `Description`, and `Type`. Built via static factories: `Error.Failure()`, `Error.Validation()`, `Error.NotFound()`, `Error.Conflict()`, `Error.Unauthorized()`, `Error.Forbidden()`. `Error.None` is the sentinel carried by successful results; the result constructor compares against it **by reference**, and rejects `null` errors
- **`Result<TValue>`** — base class with `IsSuccess`/`IsFailure`/`Error`/`Value` (`Value` throws if accessed on failure). Supports `Match` for pattern matching
- **`Result`** — derives from `Result<Result>` (Ardalis.Result-style) for operations without a value; its `Value` always throws. Created via `Result.Success()`, `Result.Success<T>(value)`, `Result.Failure(error)`

### Implicit conversion convention

```csharp
Result<string> r1 = "hello";                                    // success
Result<string> r2 = Error.NotFound("X.NotFound", "Not found");  // failure
Result         r3 = Error.Failure("X.Failed", "Failed");        // failure
Result<string> r4 = Result.Failure(error);                      // failed Result → Result<T> (throws if successful)
```

C# ignores user-defined conversions from interface types, so for `Result<IEnumerable<T>>` etc. use `Result.Success(value)`.

### Error code naming convention

Dot notation for domain-scoped codes: `"Product.NotFound"`, `"User.EmailConflict"`. Generic codes use UPPER_SNAKE_CASE: `"INVALID_EMAIL"`.

### Clean Architecture sample (`samples/CleanArchitecture/`)

Demonstrates the intended usage pattern:
- **Application layer**: services return `Result<T>` — no exceptions thrown for business failures. Errors are defined once per feature in a static class (`ProductErrors`) and returned via implicit conversion
- **Web API layer**: controllers call `result.Match(...)`; failures go through `this.ToProblem(error)` (`WebApi/Extensions/ErrorHttpExtensions.cs`), the single place that maps `ErrorType` → HTTP status and emits problem details with the error `code`

### Multi-targeting

The library targets .NET 8, 9, and 10 simultaneously (configured in `src/Directory.Build.props`). The test project multi-targets the same three frameworks, so CI tests all three; samples target .NET 10 only. The SDK is pinned via `global.json`.

### Build configuration

- **`Directory.Build.props`** (root) — shared by all projects: nullable, implicit usings, `TreatWarningsAsErrors`, `IsPackable=false` by default, `ContinuousIntegrationBuild` on GitHub Actions
- **`src/Directory.Build.props`** — imports the root file; adds target frameworks, `IsPackable=true` and NuGet metadata
- **`Directory.Packages.props`** (root) — central package management; add versions here, not in `.csproj` files
- **Package validation** runs on `dotnet pack` against `PackageValidationBaselineVersion` in `Arcanic.Result.csproj`. After each release, bump `VersionPrefix` to the next version and the baseline to the version just shipped
