using CleanArchitecture.Application.Product;
using Microsoft.AspNetCore.Mvc;
using Arcanic.Result;
using CleanArchitecture.WebApi.Models.Product;

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

            if (result.IsSuccess)
            {
                var productsList = result.Value.Select(product => new ProductDetails
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price
                }).ToList();

                return Ok(productsList);
            }

            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error),
                _ => Problem(result.Error.Description)
            };
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Get(int Id)
        {
            var result = await productService.GetByIdAsync(Id);

            if (result.IsSuccess)
            {
                var productDetails = new ProductDetails
                {
                    Id = result.Value.Id,
                    Name = result.Value.Name,
                    Price = result.Value.Price
                };
                return Ok(productDetails);
            }

            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => Problem(result.Error.Description)
            };
        }

        [HttpPost()]
        public async Task<IActionResult> Add([FromBody] CreateProduct request)
        {
            var result = await productService.CreateAsync(request.Name, request.Description, request.Price);

            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(Get), new { Id = result.Value }, new { Id = result.Value });
            }

            return result.Error.Type switch
            {
                ErrorType.Validation => BadRequest(result.Error),
                _ => Problem(result.Error.Description)
            };
        }

        [HttpPut("{id}/price")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdateProductPrice request)
        {
            var result = await productService.UpdatePriceAsync(id, request.Price);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => Problem(result.Error.Description)
            };
        }
    }
}
