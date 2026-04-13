using RecipeBookWebApi.Models;
using RecipeBookWebApi.Dto;

namespace RecipeBookWebApi.Services;

public interface IDishService
{
    Task<List<DishResponseDto>> GetDishesAsync(DishFilter filter);
    Task<DishResponseDto?> GetDishByIdAsync(int id);
    Task<DishResponseDto> CreateDishAsync(DishRequestDto data);
    Task UpdateDishAsync(int id, DishRequestDto data);
    Task DeleteDishAsync(int id);
    
    Task<NutritionResponseDto> CalculateNutritionAsync(NutritionCalculateRequestDto data);
}
