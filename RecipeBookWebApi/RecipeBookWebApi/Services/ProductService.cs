using Microsoft.EntityFrameworkCore;
using RecipeBookWebApi.Data;
using RecipeBookWebApi.Models;
using RecipeBookWebApi.Dto;

namespace RecipeBookWebApi.Services;

public class ProductService : IProductService
{
    private readonly RecipeBookContext _context;

    public ProductService(RecipeBookContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetProductsAsync(ProductFilter filter)
    {
        var searchValue = filter.Search?.Trim().ToLower();
        var query = _context.Products
            .Where(p => !filter.Category.HasValue || p.Category == filter.Category.Value)
            .Where(p => !filter.CookingRequired.HasValue || p.CookingRequired == filter.CookingRequired.Value)
            .Where(p => !filter.Flags.HasValue || p.Flags.HasFlag(filter.Flags.Value))
            .Where(p => string.IsNullOrEmpty(searchValue) || EF.Functions.Like(p.Name.ToLower(), $"%{searchValue}%"));

        query = filter.SortBy switch
            {
                ProductSortParam.Name => query.OrderBy(p => p.Name),
                ProductSortParam.Calories => query.OrderBy(p => p.Calories),
                ProductSortParam.Proteins => query.OrderBy(p => p.Proteins),
                ProductSortParam.Fats => query.OrderBy(p => p.Fats),
                ProductSortParam.Carbs => query.OrderBy(p => p.Carbs),
                _ => query
            };

        return await query.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        try
        {
            return await _context.Products.FirstAsync(p => p.Id == id);
        }
        catch
        {
            throw new KeyNotFoundException("Product not found");
        }
    }

    public async Task<Product> CreateProductAsync(ProductRequestDto data)
    {
        try
        {
            Product product = new Product()
            {
                Name = data.Name,
                Photos = data.Photos ?? [],
                Calories = data.Calories ?? 0,
                Proteins = data.Proteins ?? 0,
                Fats = data.Fats ?? 0,
                Carbs = data.Carbs ?? 0,
                Composition = data.Composition,
                Category = data.Category!.Value,
                CookingRequired = data.CookingRequired!.Value,
                Flags = data.Flags
            };
            
            ValidateProductNutrition(product);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            
            return product;
        }
        catch
        {
            throw;
        }
        
    }

    public async Task UpdateProductAsync(int id, ProductRequestDto data)
    {
        Product product = await GetProductByIdAsync(id);
        
        product.Name = data.Name;
        product.Photos = data.Photos;
        product.Calories = data.Calories ?? 0;
        product.Proteins = data.Proteins ?? 0;
        product.Fats = data.Fats ?? 0;
        product.Carbs = data.Carbs ?? 0;
        product.Composition = data.Composition;
        product.Category = data.Category!.Value;
        product.CookingRequired = data.CookingRequired!.Value;
        product.Flags = data.Flags;
        product.UpdatedAt = DateTime.UtcNow;
        
        ValidateProductNutrition(product);

        await _context.SaveChangesAsync();

        // Recalculate flags for dishes using this product
        var dishesToUpdate = await _context.Dishes
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Product)
            .Where(d => d.Ingredients.Any(i => i.ProductId == id))
            .ToListAsync();

        foreach (var dish in dishesToUpdate)
        {
            // Only remove flags that are no longer valid based on ingredients
            if (dish.Ingredients.Any(i => !i.Product.Flags.HasFlag(ProductFlags.Веган)))
                dish.Flags &= ~DishFlags.Веган;
            if (dish.Ingredients.Any(i => !i.Product.Flags.HasFlag(ProductFlags.БезГлютена)))
                dish.Flags &= ~DishFlags.БезГлютена;
            if (dish.Ingredients.Any(i => !i.Product.Flags.HasFlag(ProductFlags.БезСахара)))
                dish.Flags &= ~DishFlags.БезСахара;
            dish.UpdatedAt = DateTime.UtcNow;
        }

        if (dishesToUpdate.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            Product product = await GetProductByIdAsync(id);

            var isUsedInDishes = await _context.DishIngredients.AnyAsync(di => di.ProductId == id);
            if (isUsedInDishes)
            {
                var dishes = await _context.Dishes
                    .Where(d => d.Ingredients.Any(di => di.ProductId == id))
                    .Select(d => d.Name)
                    .ToListAsync();

                throw new InvalidOperationException($"Cannot delete product used in dishes: {string.Join(", ", dishes)}");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(); 
        }
        catch 
        {
            throw;
        }
        
    }

    private static void ValidateProductNutrition(Product product)
    {
        if (product.Proteins + product.Fats + product.Carbs > 100)
            throw new InvalidOperationException("Sum of Proteins, Fats, Carbs cannot exceed 100");
    }
}
