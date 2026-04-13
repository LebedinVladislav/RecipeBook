using System.ComponentModel.DataAnnotations;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Dto;


public class ProductRequestDto
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

    public string? Composition { get; set; }

    [Required]
    public ProductCategory? Category { get; set; }

    [Required]
    public CookingRequired? CookingRequired { get; set; }
    
    public ProductFlags Flags { get; set; } = ProductFlags.None;
}