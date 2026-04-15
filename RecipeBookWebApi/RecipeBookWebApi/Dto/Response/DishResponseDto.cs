using System.ComponentModel.DataAnnotations;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Dto;


public class DishResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<string> Photos { get; set; } = [];

    public double Calories { get; set; }

    public double Proteins { get; set; }

    public double Fats { get; set; }

    public double Carbs { get; set; }

    public List<DishIngredientDto> Ingredients { get; set; } = [];

    public double PortionSize { get; set; }

    public DishCategory Category { get; set; }

    public DishFlags Flags { get; set; } = DishFlags.None;

    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}