using Arcanic.Result;

namespace CleanArchitecture.Application.Product;

/// <summary>
/// Every error the product feature can produce, defined once so codes stay consistent
/// and callers (and tests) can compare against them.
/// </summary>
public static class ProductErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("Product.NotFound", $"Product with ID {id} was not found.");

    public static readonly Error NameEmpty =
        Error.Validation("Product.Name.Empty", "Product name cannot be empty.");

    public static readonly Error PriceNegative =
        Error.Validation("Product.Price.Negative", "Product price cannot be negative.");
}
