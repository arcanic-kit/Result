<div align="center">

<img src="https://raw.githubusercontent.com/arcanic-kit/Result/main/assets/images/logo.png" alt="Arcanic.Result" width="120" />

# Arcanic.Result

**Explicit, type-safe error handling for .NET — no exceptions for business failures.**

[![NuGet](https://img.shields.io/nuget/v/Arcanic.Result?style=for-the-badge&logo=nuget&logoColor=white&label=NuGet&color=004880)](https://www.nuget.org/packages/Arcanic.Result)
[![Downloads](https://img.shields.io/nuget/dt/Arcanic.Result?style=for-the-badge&logo=nuget&logoColor=white&label=Downloads&color=004880)](https://www.nuget.org/packages/Arcanic.Result)
[![Build](https://img.shields.io/github/actions/workflow/status/arcanic-kit/Result/build-and-test.yml?style=for-the-badge&logo=githubactions&logoColor=white&label=Build)](https://github.com/arcanic-kit/Result/actions/workflows/build-and-test.yml)
[![.NET](https://img.shields.io/badge/.NET-10%20%7C%2011-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-MIT-22C55E?style=for-the-badge)](https://github.com/arcanic-kit/Result/blob/main/LICENSE)

</div>

---

## ✨ Features

- ✅ **`Result` and `Result<T>`**: a method either succeeds or fails, and the caller has to handle both
- 🏷️ **Structured errors** with a code, a description and a type (`NotFound`, `Validation`, ...)
- 🔀 **Implicit conversions**: `return value;` or `return error;`, no wrapping
- 🔗 **Railway-style chaining** with `Map`, `Bind`, `Ensure`, `Tap`
- ⚡ **Async pipelines** that read like sync code
- 🧩 **Multiple errors** for reporting every validation failure at once
- 🪶 **Zero dependencies**, targets .NET 10 and 11

## 📦 Installation

```bash
dotnet add package Arcanic.Result
```

## 🚀 Quick start

```csharp
using Arcanic.Result;

public Result<int> Parse(string input) =>
    int.TryParse(input, out var number)
        ? number                                                    // ✅ success
        : Error.Validation("Number.Invalid", "Not a valid number"); // ❌ failure

string message = Parse("42").Match(
    onSuccess: number => $"Got {number}",
    onFailure: error  => $"Oops: {error.Description}");
```

## 🏷️ Errors

```csharp
Error.Failure("Db.Timeout",         "The database timed out");
Error.Validation("Email.Empty",     "Email is required");
Error.NotFound("User.NotFound",     "User does not exist");
Error.Conflict("Email.Taken",       "This email is already used");
Error.Unauthorized("Auth.Required", "Authentication is required");
Error.Forbidden("Role.Missing",     "You are not allowed to do this");
```

> 💡 Use dot notation for domain error codes (`Product.NotFound`) and keep each feature's errors in one static class.

## 🔗 Chaining

Each step runs only on success. On failure the steps are skipped, and the failure is passed along with all of its errors.

```csharp
Result<string> greeting = ParseId(input)              // Result<int>
    .Bind(users.Find)                                 // Result<User>
    .Ensure(user => user.IsActive, UserErrors.Inactive)
    .Tap(user => logger.LogInformation("Found {Id}", user.Id))
    .Map(user => $"Hello, {user.Name}!");
```

| Method | What it does |
|---|---|
| `Map` | 🔄 transforms the value |
| `Bind` | 🔗 chains an operation that can fail |
| `Ensure` | 🛡️ fails when a condition is not met |
| `Tap` / `TapError` | 👀 runs a side effect on success / on failure |
| `Match` | 🎯 handles both outcomes and returns a value |

## ⚡ Async

The same names work on a `Task<Result<T>>`, and the `...Async` versions take async functions.

```csharp
string message = await users.FindAsync(id)            // Task<Result<User>>
    .Ensure(user => user.IsActive, UserErrors.Inactive)
    .BindAsync(user => mailer.SendWelcomeAsync(user))  // Task<Result>
    .Match(() => "Welcome email sent 📧", error => error.Description);
```

## 🧩 Multiple errors

```csharp
Result validation = Result.Combine(ValidateName(name), ValidatePrice(price));

if (validation.IsFailure)
{
    foreach (var error in validation.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}");
}
```

`Errors` lists every error of a failure. `Error` is the first one.

## 🏗️ Sample

The [Clean Architecture sample](https://github.com/arcanic-kit/Result/tree/main/samples/CleanArchitecture) is a complete Web API. Services return `Result<T>`, and every error becomes an RFC 9457 problem details response.

## 🔢 Versioning

The major version follows .NET (**11.x for .NET 11**), so a minor release can contain breaking changes. Check the [release notes](https://github.com/arcanic-kit/Result/tree/main/.github/release-notes) before upgrading.

> ⚠️ **Upgrading from 10.0 to 10.1?** In methods returning `Result<T>`, replace `Result.Failure(error)` with `return error;` or `Result.Failure<T>(error)`.

## 📄 License

[MIT](https://github.com/arcanic-kit/Result/blob/main/LICENSE) © Intello Solutions
