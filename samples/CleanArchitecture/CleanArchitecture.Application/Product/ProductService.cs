using Arcanic.Result;
using CleanArchitecture.Application.Product.Dtos;
using CleanArchitecture.Domain.Products;

namespace CleanArchitecture.Application.Product;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return Result.Failure<ProductDto>(
                Error.NotFound("Product.NotFound", $"Product with ID {id} was not found."));
        }

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };

        return Result.Success(productDto);
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        var productDtos = products.Select(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        });

        return Result.Success(productDtos);
    }

    public async Task<Result<int>> CreateAsync(string name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<int>(
                Error.Validation("Product.Name.Empty", "Product name cannot be empty."));
        }

        if (price < 0)
        {
            return Result.Failure<int>(
                Error.Validation("Product.Price.Negative", "Product price cannot be negative."));
        }

        var product = new Domain.Products.Product(0, name, price)
        {
            Description = description
        };

        var createdProduct = await _productRepository.AddAsync(product);
        return Result.Success(createdProduct.Id);
    }

    public async Task<Result> UpdatePriceAsync(int id, decimal price)
    {
        if (price < 0)
        {
            return Result.Failure(
                Error.Validation("Product.Price.Negative", "Product price cannot be negative."));
        }

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return Result.Failure(
                Error.NotFound("Product.NotFound", $"Product with ID {id} was not found."));
        }

        product.UpdatePrice(price);
        await _productRepository.UpdateAsync(product);

        return Result.Success();
    }
}
