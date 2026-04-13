using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Services;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync(ProductFilter filter);
    Task<Product> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(ProductRequestDto data);
    Task UpdateProductAsync(int id, ProductRequestDto data);
    Task DeleteProductAsync(int id);
}
