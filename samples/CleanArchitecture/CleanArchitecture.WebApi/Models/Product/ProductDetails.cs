using CleanArchitecture.Application.Product.Dtos;

namespace CleanArchitecture.WebApi.Models.Product;

public class ProductDetails
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public static ProductDetails From(ProductDto product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price
    };
}
