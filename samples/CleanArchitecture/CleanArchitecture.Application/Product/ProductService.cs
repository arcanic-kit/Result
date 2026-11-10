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

    public Task<Result<ProductDto>> GetByIdAsync(int id) =>
        FindAsync(id).Map(ToDto);

    public async Task<Result<IEnumerable<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        // C# does not apply user-defined conversions from interface types such as
        // IEnumerable<T>, so this case uses Result.Success explicitly.
        return Result.Success(products.Select(ToDto));
    }

    public async Task<Result<int>> CreateAsync(string name, string? description, decimal price)
    {
        // Report every validation problem at once instead of stopping at the first.
        var validation = Result.Combine(ValidateName(name), ValidatePrice(price));

        if (validation.IsFailure)
        {
            return Result.Failure<int>(validation.Errors);
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
        var validation = ValidatePrice(price);

        if (validation.IsFailure)
        {
            return validation;
        }

        return await FindAsync(id).BindAsync(async product =>
        {
            product.UpdatePrice(price);
            await _productRepository.UpdateAsync(product);
            return Result.Success();
        });
    }

    private async Task<Result<Domain.Products.Product>> FindAsync(int id) =>
        await _productRepository.GetByIdAsync(id) is { } product
            ? product
            : ProductErrors.NotFound(id);

    private static Result ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name) ? ProductErrors.NameEmpty : Result.Success();

    private static Result ValidatePrice(decimal price) =>
        price < 0 ? ProductErrors.PriceNegative : Result.Success();

    private static ProductDto ToDto(Domain.Products.Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price
    };
}
