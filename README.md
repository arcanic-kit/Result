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

Instead of throwing an exception when something expected goes wrong (a user that doesn't exist, an invalid email…), your method **returns a `Result`**. The caller sees from the signature that it can fail, and has to handle both outcomes.

## 📦 Installation

```bash
dotnet add package Arcanic.Result
```

Zero dependencies. Targets .NET 10 and .NET 11.

## ✅ Return a result

Declare `Result<T>` as the return type, then return `Result.Success(value)` on success or `Result.Failure<T>(error)` on failure:

```csharp
using Arcanic.Result;

public Result<User> GetUser(int id)
{
    var user = _users.Find(id);

    if (user is null)
    {
        return Result.Failure<User>(Error.NotFound("User.NotFound", $"User {id} was not found")); // ❌ failure
    }

    return Result.Success(user); // ✅ success
}
```

When there is no value to return, use `Result`, `Result.Success()` and `Result.Failure(error)`:

```csharp
public Result Deactivate(User user)
{
    if (!user.IsActive)
    {
        return Result.Failure(Error.Conflict("User.AlreadyInactive", "The user is already inactive"));
    }

    user.IsActive = false;
    return Result.Success();
}
```

It works the same in async methods: return `Task<Result<T>>` or `Task<Result>`.

> 💡 Shorthand: `return user;` and `return error;` work too, because values and errors convert to `Result<User>` implicitly. The value conversion doesn't work for interface types such as `Result<IEnumerable<T>>`, so use `Result.Success(items)` there.

## 🎯 Handle the result

`Match` makes you handle both outcomes:

```csharp
string message = GetUser(42).Match(
    onSuccess: user  => $"Hello, {user.Name}!",
    onFailure: error => error.Description);
```

Or check it yourself:

```csharp
Result<User> result = GetUser(42);

if (result.IsFailure)
{
    Console.WriteLine($"{result.Error.Code}: {result.Error.Description}");
    return;
}

User user = result.Value;
```

## 🏷️ Errors

An error has a **code**, a **description** and a **type**. Pick the type that describes the failure:

```csharp
Error.Validation("Email.Empty",     "Email is required");
Error.NotFound("User.NotFound",     "User does not exist");
Error.Conflict("Email.Taken",       "This email is already used");
Error.Unauthorized("Auth.Required", "Authentication is required");
Error.Forbidden("Role.Missing",     "You are not allowed to do this");
Error.Failure("Db.Timeout",         "The database timed out");
```

Keep each feature's errors in one static class, and wrap them in a failure:

```csharp
public static class UserErrors
{
    public static readonly Error Inactive = Error.Conflict("User.Inactive", "The user is inactive");

    public static Error NotFound(int id) => Error.NotFound("User.NotFound", $"User {id} was not found");
}

return Result.Failure<User>(UserErrors.NotFound(id));
```

## 🚀 Going further

- 🔗 **Chaining**: `Map`, `Bind`, `Ensure`, `Tap` and `TapError` run the next step only on success.
- ⚡ **Async pipelines**: the same methods work on `Task<Result<T>>`, plus `MapAsync`, `BindAsync`, `EnsureAsync`, `TapAsync`.
- 🧩 **Multiple errors**: `Result.Combine(...)` reports every validation failure at once in `Errors`.

The [Clean Architecture sample](https://github.com/arcanic-kit/Result/tree/main/samples/CleanArchitecture) shows all of this in a complete Web API, where every error becomes an RFC 9457 problem details response.

## 🔢 Versioning

The major version follows .NET (**11.x for .NET 11**), so a minor release can contain breaking changes. Check the [release notes](https://github.com/arcanic-kit/Result/tree/main/.github/release-notes) before upgrading. Still on .NET 8 or 9? Use **10.1.0**.

## 📄 License

[MIT](https://github.com/arcanic-kit/Result/blob/main/LICENSE) © Intello Solutions
