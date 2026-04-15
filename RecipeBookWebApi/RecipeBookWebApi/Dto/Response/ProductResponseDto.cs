using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Dto;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = [];
    public double Calories { get; set; }
    public double Proteins { get; set; }
    public double Fats { get; set; }
    public double Carbs { get; set; }
    public string? Composition { get; set; }
    public ProductCategory Category { get; set; }
    public CookingRequired CookingRequired { get; set; }
    public ProductFlags Flags { get; set; } = ProductFlags.None;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}