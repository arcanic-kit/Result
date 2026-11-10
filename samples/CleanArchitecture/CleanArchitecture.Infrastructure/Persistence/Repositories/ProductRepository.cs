using CleanArchitecture.Domain.Products;

namespace CleanArchitecture.Infrastructure.Persistence.Repositories;

/// <summary>
/// In-memory product store. Registered as a singleton, so every access is guarded by <see cref="_lock"/>.
/// </summary>
internal class ProductRepository : IProductRepository
{
    private readonly Lock _lock = new();
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
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product);
        }
    }

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Product>>(_products.ToList());
        }
    }

    public Task<Product> AddAsync(Product product)
    {
        lock (_lock)
        {
            // Create a new product with the next available ID
            var newProduct = new Product(_nextId++, product.Name, product.Price)
            {
                Description = product.Description
            };
            _products.Add(newProduct);
            return Task.FromResult(newProduct);
        }
    }

    public Task UpdateAsync(Product product)
    {
        lock (_lock)
        {
            var existingIndex = _products.FindIndex(p => p.Id == product.Id);
            if (existingIndex >= 0)
            {
                _products[existingIndex] = product;
            }
            return Task.CompletedTask;
        }
    }

    public Task DeleteAsync(int id)
    {
        lock (_lock)
        {
            _products.RemoveAll(p => p.Id == id);
            return Task.CompletedTask;
        }
    }
}
