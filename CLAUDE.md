# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore dependencies
dotnet restore Arcanic.Result.slnx

# Build
dotnet build Arcanic.Result.slnx --configuration Release --no-restore

# Tests use xunit.v3 on Microsoft Testing Platform (opted in via global.json), so
# dotnet test takes --solution/--project and MTP options, not VSTest ones (--filter, --collect).

# Run all tests
dotnet test --solution Arcanic.Result.slnx --configuration Release --no-build

# Run tests with coverage (coverlet.MTP, Cobertura output)
dotnet test --solution Arcanic.Result.slnx --configuration Release --no-build --results-directory ./coverage --coverlet --coverlet-output-format cobertura

# Run a single test class (xunit filters: --filter-class, --filter-method, --filter-trait)
dotnet test --project tests/Arcanic.Result.Tests --filter-class "Arcanic.Result.Tests.ErrorTests"

# Pack NuGet package
dotnet pack Arcanic.Result.slnx --configuration Release --no-build --output ./packages
```

## Architecture

This is a **Railway-Oriented Programming** library for .NET 8/9/10 that provides explicit success/failure result types. It avoids exceptions for business logic control flow.

### Core abstractions (`src/Arcanic.Result/`)

- **`ErrorType`** — enum classifying errors: `Failure`, `Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden`
- **`Error`** — sealed record with `Code`, `Description`, and `Type`. Built via static factories: `Error.Failure()`, `Error.Validation()`, `Error.NotFound()`, `Error.Conflict()`, `Error.Unauthorized()`, `Error.Forbidden()`. `Error.None` is the sentinel carried by successful results; the result constructor compares against it **by reference**, and rejects `null` errors
- **`Result`** — base class with `IsSuccess`/`IsFailure`/`Error`; the validating constructor is `private protected`, so `Result<TValue>` is the only subclass. Created via `Result.Success()`, `Result.Success<T>(value)`, `Result.Failure(error)`, `Result.Failure<T>(error)`. Supports `Match` for pattern matching
- **`Result<TValue>`** — derives from `Result`, adding `Value` (throws if accessed on failure)
- **Multiple errors** — a failure carries one or more errors: `Errors` lists all of them (empty on success), `Error` is the first (`Error.None` on success). Create with `Result.Failure(IEnumerable<Error>)` / `Result.Failure<T>(IEnumerable<Error>)`; `Result.Combine(...)` merges results, collecting every error. `Match`'s `onFailure` receives the first error

- **`ResultExtensions`** — railway combinators `Map`, `Bind`, `Ensure`, `Tap`, `TapError`; each runs only on success and otherwise propagates the failure with all its errors (`Result.Failure<T>(result.Errors)`)
- **`ResultAsyncExtensions`** — same names on `Task<Result<T>>`/`Task<Result>` take sync functions; `...Async` names take async functions. Library awaits use `ConfigureAwait(false)`

### Implicit conversion convention

```csharp
Result<string> r1 = "hello";                                    // success
Result<string> r2 = Error.NotFound("X.NotFound", "Not found");  // failure
Result         r3 = Error.Failure("X.Failed", "Failed");        // failure
Result         r4 = r1;                                         // any Result<T> is a Result
```

There is no conversion from `Result` to `Result<T>` (C# forbids user-defined conversions from a base class); use `Result.Failure<T>(error)` or return the `Error` directly. C# also ignores user-defined conversions from interface types, so for `Result<IEnumerable<T>>` etc. use `Result.Success(value)`.

### Breaking changes

`src/Arcanic.Result/CompatibilitySuppressions.xml` lists the breaks accepted for the current major version against `PackageValidationBaselineVersion`. Only regenerate it (`dotnet pack -p:ApiCompatGenerateSuppressionFile=true`) for an intentional major-version break, and delete it when the baseline moves to a version that already contains those breaks.

### Error code naming convention

Dot notation for domain-scoped codes: `"Product.NotFound"`, `"User.EmailConflict"`. Generic codes use UPPER_SNAKE_CASE: `"INVALID_EMAIL"`.

### Clean Architecture sample (`samples/CleanArchitecture/`)

Demonstrates the intended usage pattern:
- **Application layer**: services return `Result<T>` — no exceptions thrown for business failures. Errors are defined once per feature in a static class (`ProductErrors`) and returned via implicit conversion. Validation uses `Result.Combine` to report every error; lookups chain with `Map`/`BindAsync`
- **Web API layer**: controllers call `result.Match(...)`; failures go through `this.ToProblem(result)` (`WebApi/Extensions/ErrorHttpExtensions.cs`), the single place that maps `ErrorType` → HTTP status and emits problem details with `code` and every error in `errors`. It takes the result, not an `Error`, because `Match`'s `onFailure` only receives the first error

### Multi-targeting

The library targets .NET 8, 9, and 10 simultaneously (configured in `src/Directory.Build.props`). The test project multi-targets the same three frameworks, so CI tests all three; samples target .NET 10 only. The SDK is pinned via `global.json`.

### Build configuration

- **`Directory.Build.props`** (root) — shared by all projects: nullable, implicit usings, `TreatWarningsAsErrors`, `IsPackable=false` by default, `ContinuousIntegrationBuild` on GitHub Actions
- **`src/Directory.Build.props`** — imports the root file; adds target frameworks, `IsPackable=true` and NuGet metadata
- **`Directory.Packages.props`** (root) — central package management; add versions here, not in `.csproj` files
- **Package validation** runs on `dotnet pack` against `PackageValidationBaselineVersion` in `Arcanic.Result.csproj`. After each release, bump `VersionPrefix` to the next version and the baseline to the version just shipped
