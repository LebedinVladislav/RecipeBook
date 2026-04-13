using System.ComponentModel.DataAnnotations;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Dto;

public class NutritionCalculateRequestDto
{
    [Required]
    [MinLength(1)]
    public List<DishIngredientDto> Ingredients { get; set; } = [];
}
