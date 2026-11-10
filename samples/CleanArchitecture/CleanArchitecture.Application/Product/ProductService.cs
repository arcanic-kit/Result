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

        if (product is null)
        {
            // Error converts implicitly to a failed Result<ProductDto>
            return ProductErrors.NotFound(id);
        }

        // ProductDto converts implicitly to a successful Result<ProductDto>
        return ToDto(product);
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        // C# does not apply user-defined conversions from interface types such as
        // IEnumerable<T>, so this case uses Result.Success explicitly.
        return Result.Success(products.Select(ToDto));
    }

    public async Task<Result<int>> CreateAsync(string name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ProductErrors.NameEmpty;
        }

        if (price < 0)
        {
            return ProductErrors.PriceNegative;
        }

        var product = new Domain.Products.Product(0, name, price)
        {
            Description = description
        };

        var createdProduct = await _productRepository.AddAsync(product);
        return createdProduct.Id;
    }

    public async Task<Result> UpdatePriceAsync(int id, decimal price)
    {
        if (price < 0)
        {
            return ProductErrors.PriceNegative;
        }

        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return ProductErrors.NotFound(id);
        }

        product.UpdatePrice(price);
        await _productRepository.UpdateAsync(product);

        return Result.Success();
    }

    private static ProductDto ToDto(Domain.Products.Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price
    };
}
