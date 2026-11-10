using CleanArchitecture.Application.Product;
using CleanArchitecture.WebApi.Extensions;
using CleanArchitecture.WebApi.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController(IProductService productService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await productService.GetAllAsync();

            return result.Match(
                onSuccess: products => Ok(products.Select(ProductDetails.From).ToList()),
                onFailure: _ => this.ToProblem(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await productService.GetByIdAsync(id);

            return result.Match(
                onSuccess: product => Ok(ProductDetails.From(product)),
                onFailure: _ => this.ToProblem(result));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateProduct request)
        {
            var result = await productService.CreateAsync(request.Name, request.Description, request.Price);

            return result.Match(
                onSuccess: id => CreatedAtAction(nameof(Get), new { id }, new { id }),
                onFailure: _ => this.ToProblem(result));
        }

        [HttpPut("{id}/price")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdateProductPrice request)
        {
            var result = await productService.UpdatePriceAsync(id, request.Price);

            return result.Match(
                onSuccess: () => NoContent(),
                onFailure: _ => this.ToProblem(result));
        }
    }
}
