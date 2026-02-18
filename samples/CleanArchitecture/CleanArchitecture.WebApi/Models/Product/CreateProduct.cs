namespace CleanArchitecture.WebApi.Models.Product;

public class CreateProduct
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}