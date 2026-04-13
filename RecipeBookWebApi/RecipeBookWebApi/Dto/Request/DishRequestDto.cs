using System.ComponentModel.DataAnnotations;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Dto;


public class DishRequestDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(5)]
    public List<string> Photos { get; set; } = [];

    [Required]
    [Range(0, double.MaxValue)]
    public double? Calories { get; set; }

    [Required]
    [Range(0, 100)]
    public double? Proteins { get; set; }

    [Required]
    [Range(0, 100)]
    public double? Fats { get; set; }

    [Required]
    [Range(0, 100)]
    public double? Carbs { get; set; }

    [Required]
    [MinLength(1)]
    public List<DishIngredientDto> Ingredients { get; set; } = [];

    [Required]
    [Range(0.1, double.MaxValue)]
    public double? PortionSize { get; set; }

    public DishCategory? Category { get; set; }

    public DishFlags Flags { get; set; } = DishFlags.None;

}