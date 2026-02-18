using CleanArchitecture.Domain.Products;

namespace CleanArchitecture.Infrastructure.Persistence.Repositories;

internal class ProductRepository : IProductRepository
{
    private readonly List<Product> _products;
    private int _nextId;

    public ProductRepository()
    {
        _products = new List<Product>();
        _nextId = 1;

        // Add default products
        var laptop = new Product(_nextId++, "Gaming Laptop", 1299.99m)
        {
            Description = "High-performance gaming laptop with RTX 4060"
        };
        _products.Add(laptop);

        var mouse = new Product(_nextId++, "Wireless Mouse", 29.99m)
        {
            Description = "Ergonomic wireless mouse with precision tracking"
        };
        _products.Add(mouse);

        var keyboard = new Product(_nextId++, "Mechanical Keyboard", 89.99m)
        {
            Description = "RGB mechanical keyboard with blue switches"
        };
        _products.Add(keyboard);

        var monitor = new Product(_nextId++, "4K Monitor", 399.99m)
        {
            Description = "27-inch 4K UHD monitor with HDR support"
        };
        _products.Add(monitor);

        var headphones = new Product(_nextId++, "Noise-Cancelling Headphones", 199.99m)
        {
            Description = "Premium wireless headphones with active noise cancellation"
        };
        _products.Add(headphones);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Product>>(_products.ToList());
    }

    public Task<Product> AddAsync(Product product)
    {
        // Create a new product with the next available ID
        var newProduct = new Product(_nextId++, product.Name, product.Price)
        {
            Description = product.Description
        };
        _products.Add(newProduct);
        return Task.FromResult(newProduct);
    }

    public Task UpdateAsync(Product product)
    {
        var existingIndex = _products.FindIndex(p => p.Id == product.Id);
        if (existingIndex >= 0)
        {
            _products[existingIndex] = product;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }
}
