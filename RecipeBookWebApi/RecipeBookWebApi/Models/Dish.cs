using System.ComponentModel.DataAnnotations;

namespace RecipeBookWebApi.Models;

public enum DishCategory
{
    Десерт,
    Первое,
    Второе,
    Напиток,
    Салат,
    Суп,
    Перекус
}

[Flags]
public enum DishFlags
{
    None = 0,
    Веган = 1,
    БезГлютена = 2,
    БезСахара = 4
}

public class DishFilter
{
    public DishCategory? Category { get; set; }
    public DishFlags? Flags { get; set; }
    public string? Search { get; set; }
}

public class Dish
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<string> Photos { get; set; } = [];

    public double Calories { get; set; }

    public double Proteins { get; set; }

    public double Fats { get; set; }

    public double Carbs { get; set; }

    public List<DishIngredient> Ingredients { get; set; } = [];

    public double PortionSize { get; set; }

    public DishCategory Category { get; set; }

    public DishFlags Flags { get; set; } = DishFlags.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}