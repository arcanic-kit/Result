using Arcanic.Result;
using CleanArchitecture.Application.Product.Dtos;

namespace CleanArchitecture.Application.Product;

public interface IProductService
{
    Task<Result<ProductDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<ProductDto>>> GetAllAsync();
    Task<Result<int>> CreateAsync(string name, string? description, decimal price);
    Task<Result> UpdatePriceAsync(int id, decimal price);
}
