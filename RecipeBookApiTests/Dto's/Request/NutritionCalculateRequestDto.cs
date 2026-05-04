using System.ComponentModel.DataAnnotations;

namespace RecipeBookApiTests.Dto;

public class NutritionCalculateRequestDto
{
    [Required]
    [MinLength(1)]
    public List<DishIngredientDto> Ingredients { get; set; } = [];
    public double PortionSize { get; set; }
}
