using Microsoft.AspNetCore.Mvc;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;
using RecipeBookWebApi.Services;

namespace RecipeBookWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts([FromQuery] ProductFilter filter)
    {
        var products = await _productService.GetProductsAsync(filter);
        return Ok(products.Select(ToResponseDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        try
        {
            Product product = await _productService.GetProductByIdAsync(id);
            return Ok(ToResponseDto(product));
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromBody] ProductRequestDto data)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _productService.CreateProductAsync(data);
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, ToResponseDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDto data)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _productService.UpdateProductAsync(id, data);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static ProductResponseDto ToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Photos = product.Photos,
            Calories = product.Calories,
            Proteins = product.Proteins,
            Fats = product.Fats,
            Carbs = product.Carbs,
            Composition = product.Composition,
            Category = product.Category,
            CookingRequired = product.CookingRequired,
            Flags = product.Flags,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}