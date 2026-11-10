using CleanArchitecture.Application;
using CleanArchitecture.Domain.Products;
using CleanArchitecture.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Infrastructure;

public static class InfrastructureExtensions
{
    public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.AddApplicationServices();

        // Singleton: the repository is an in-memory store, so a scoped lifetime
        // would discard every write at the end of the request.
        builder.Services.AddSingleton<IProductRepository, ProductRepository>();

        return builder;
    }
}