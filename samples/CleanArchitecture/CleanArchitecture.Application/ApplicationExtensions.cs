using CleanArchitecture.Application.Product;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Application;

public static class ApplicationExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductService, ProductService>();

        return builder;
    }
}