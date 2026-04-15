using Microsoft.EntityFrameworkCore;
using RecipeBookWebApi.Data;
using RecipeBookWebApi.Models;
using RecipeBookWebApi.Dto;

namespace RecipeBookWebApi.Services;

public class DishService : IDishService
{
    private readonly RecipeBookContext _context;
    private readonly IProductService _productService;

    public DishService(RecipeBookContext context, IProductService productService)
    {
        _context = context;
        _productService = productService;
    }

    public async Task<List<DishResponseDto>> GetDishesAsync(DishFilter filter)
    {
        var searchValue = filter.Search?.Trim().ToLower();
        var query = _context.Dishes
            .Include(d => d.Ingredients)
            .Where(d => !filter.Category.HasValue || d.Category == filter.Category.Value)
            .Where(d => !filter.Flags.HasValue || d.Flags.HasFlag(filter.Flags.Value))
            .Where(d => string.IsNullOrEmpty(filter.Search) || EF.Functions.Like(d.Name.ToLower(), $"%{searchValue}%"));
            
        return await query.Select(d => ToResponseDto(d)).ToListAsync();
    }

    public async Task<DishResponseDto?> GetDishByIdAsync(int id)
    {
        var dish = await _context.Dishes
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(d => d.Id == id);

        return dish == null ? null : ToResponseDto(dish);
    }

    public async Task<DishResponseDto> CreateDishAsync(DishRequestDto data)
    {
        if (data.Ingredients == null || !data.Ingredients.Any())
            throw new ArgumentException("Dish must contain at least one ingredient.", nameof(data.Ingredients));

        Dish dish = new Dish
        {
            Name = data.Name,
            Photos = data.Photos,
            Ingredients = data.Ingredients.Select(dto => new DishIngredient { ProductId = dto.ProductId, Amount = dto.Amount }).ToList(),
            PortionSize = data.PortionSize ?? 0.1,
            Flags = data.Flags,
            CreatedAt = DateTime.UtcNow,
        };

        if (data.Category.HasValue)
        {
            dish.Category = data.Category.Value;
        }
        else if (!TrySetCategoryFromName(dish))
        {
            throw new ArgumentException("Dish category is required when no macro is present in the name.", nameof(data.Category));
        }

        await FillProductsAndValidateIngredients(dish);
        
        // Рассчитываем КБЖУ на основе состава блюда
        var (calories, proteins, fats, carbs) = CalculateNutrition(dish);
        
        // Используем рассчитанные значения, если пользователь не предоставил свои
        dish.Calories = data.Calories ?? calories;
        dish.Proteins = data.Proteins ?? proteins;
        dish.Fats = data.Fats ?? fats;
        dish.Carbs = data.Carbs ?? carbs;
        
        ValidateDishNutrition(dish);
        UpdateFlags(dish);

        _context.Dishes.Add(dish);
        await _context.SaveChangesAsync();
        return ToResponseDto(dish);
    }

    private static DishResponseDto ToResponseDto(Dish dish)
    {
        return new DishResponseDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Photos = dish.Photos,
            Calories = dish.Calories,
            Proteins = dish.Proteins,
            Fats = dish.Fats,
            Carbs = dish.Carbs,
            Ingredients = dish.Ingredients.Select(i => new DishIngredientDto { ProductId = i.ProductId, Amount = i.Amount }).ToList(),
            PortionSize = dish.PortionSize,
            Category = dish.Category,
            Flags = dish.Flags,
            CreatedAt = dish.CreatedAt,
            UpdatedAt = dish.UpdatedAt
        };
    }

    public async Task UpdateDishAsync(int id, DishRequestDto data)
    {
        if (!DishExists(id))
        {
            throw new KeyNotFoundException("Dish not found");
        }

        Dish dish = await _context.Dishes.Include(d => d.Ingredients).FirstAsync(d => d.Id == id);

        dish.Name = data.Name;
        dish.Photos = data.Photos;
        dish.Ingredients = data.Ingredients.Select(dto => new DishIngredient { ProductId = dto.ProductId, Amount = dto.Amount }).ToList();
        dish.PortionSize = data.PortionSize ?? 0.1;
        dish.Flags = data.Flags;
        dish.UpdatedAt = DateTime.UtcNow;

        if (data.Category.HasValue)
        {
            dish.Category = data.Category.Value;
        }
        else if (!TrySetCategoryFromName(dish))
        {
            throw new ArgumentException("Dish category is required when no macro is present in the name.", nameof(data.Category));
        }

        await FillProductsAndValidateIngredients(dish);
        
        // Рассчитываем КБЖУ на основе состава блюда
        var (calories, proteins, fats, carbs) = CalculateNutrition(dish);
        
        // Используем рассчитанные значения, если пользователь не предоставил свои
        dish.Calories = data.Calories ?? calories;
        dish.Proteins = data.Proteins ?? proteins;
        dish.Fats = data.Fats ?? fats;
        dish.Carbs = data.Carbs ?? carbs;
        
        ValidateDishNutrition(dish);
        UpdateFlags(dish);

        var outdatedIngredients = _context.DishIngredients.Where(di => di.DishId == id);
        _context.DishIngredients.RemoveRange(outdatedIngredients);

        foreach (var ing in dish.Ingredients)
        {
            ing.DishId = id;
            await _context.DishIngredients.AddAsync(ing);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteDishAsync(int id)
    {
        var dish = await _context.Dishes.FindAsync(id);
        if (dish == null)
            throw new KeyNotFoundException("Dish not found");

        _context.Dishes.Remove(dish);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Рассчитывает КБЖУ на основе списка ингредиентов без создания блюда в БД
    /// Используется для показа черновых значений на фронтенде
    /// </summary>
    public async Task<NutritionResponseDto> CalculateNutritionAsync(NutritionCalculateRequestDto data)
    {
        if (data.Ingredients == null || !data.Ingredients.Any())
            throw new ArgumentException("At least one ingredient is required.", nameof(data.Ingredients));


        // Создаем временное блюдо только для расчета
        var tempDish = new Dish
        {
            Ingredients = [],
        };

        // Загружаем информацию о продуктах
        foreach (var ingredientDto in data.Ingredients)
        {
            if (ingredientDto.Amount <= 0)
                throw new ArgumentException($"Ingredient quantity must be positive for product {ingredientDto.ProductId}.", nameof(data.Ingredients));

            try
            {
                var product = await _productService.GetProductByIdAsync(ingredientDto.ProductId);
                tempDish.Ingredients.Add(new DishIngredient
                {
                    ProductId = ingredientDto.ProductId,
                    Amount = ingredientDto.Amount,
                    Product = product,
                });
            }
            catch
            {
                throw new ArgumentException($"Product {ingredientDto.ProductId} not found", nameof(data.Ingredients));
            }
        }

        // Рассчитываем КБЖУ
        var (calories, proteins, fats, carbs) = CalculateNutrition(tempDish);

        return new NutritionResponseDto
        {
            Calories = calories,
            Proteins = proteins,
            Fats = fats,
            Carbs = carbs
        };
    }

    private async Task FillProductsAndValidateIngredients(Dish dish)
    {
        foreach (var ing in dish.Ingredients)
        {
            if (ing.Amount <= 0)
                throw new ArgumentException($"Ingredient quantity must be positive for product {ing.ProductId}.", nameof(dish.Ingredients));

            var product = await _productService.GetProductByIdAsync(ing.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product {ing.ProductId} not found", nameof(dish.Ingredients));
            }

            ing.Product = product;
        }
    }

    private bool DishExists(int id)
    {
        return _context.Dishes.Any(e => e.Id == id);
    }

    private bool TrySetCategoryFromName(Dish dish)
    {
        var macros = new Dictionary<string, DishCategory>
        {
            {"!десерт", DishCategory.Десерт},
            {"!первое", DishCategory.Первое},
            {"!второе", DishCategory.Второе},
            {"!напиток", DishCategory.Напиток},
            {"!салат", DishCategory.Салат},
            {"!суп", DishCategory.Суп},
            {"!перекус", DishCategory.Перекус}
        };

        var lowerName = dish.Name.ToLower();
        foreach (var kvp in macros)
        {
            if (lowerName.Contains(kvp.Key))
            {
                dish.Category = kvp.Value;
                // Do not remove the macro from the name, keep it as is
                return true;
            }
        }

        return false;
    }

    private void UpdateFlags(Dish dish)
    {
        if (dish.Ingredients.Any(i => !i.Product.Flags.HasFlag(ProductFlags.Веган)))
            dish.Flags &= ~DishFlags.Веган;
        if (dish.Ingredients.Any(i => !i.Product.Flags.HasFlag(ProductFlags.БезГлютена)))
            dish.Flags &= ~DishFlags.БезГлютена;
        if (dish.Ingredients.Any(i => !i.Product.Flags.HasFlag(ProductFlags.БезСахара)))
            dish.Flags &= ~DishFlags.БезСахара;
    }

    private static void ValidateDishNutrition(Dish dish)
    {
        var totalNutrition = dish.Proteins + dish.Fats + dish.Carbs;
        if (totalNutrition > dish.PortionSize)
            throw new ArgumentException($"Sum of nutrients cannot exceed portion size.", nameof(dish));
    }

    private static (double calories, double proteins, double fats, double carbs) CalculateNutrition(Dish dish)
    {
        double totalCalories = 0;
        double totalProteins = 0;
        double totalFats = 0;
        double totalCarbs = 0;

        foreach (var ingredient in dish.Ingredients)
        {
            double amount = ingredient.Amount;
            
            totalCalories += ingredient.Product.Calories * amount / 100;
            totalProteins += ingredient.Product.Proteins * amount / 100;
            totalFats += ingredient.Product.Fats * amount / 100;
            totalCarbs += ingredient.Product.Carbs * amount / 100;
        }

        return (
            Math.Round(totalCalories, 1, MidpointRounding.AwayFromZero),
            Math.Round(totalProteins, 1, MidpointRounding.AwayFromZero),
            Math.Round(totalFats, 1, MidpointRounding.AwayFromZero),
            Math.Round(totalCarbs, 1, MidpointRounding.AwayFromZero)
        );
    }
}
