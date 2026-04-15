namespace RecipeBookWebApi.Models;

public enum ProductCategory
{
    Замороженный,
    Мясной,
    Овощи,
    Зелень,
    Специи,
    Крупы,
    Консервы,
    Жидкость,
    Сладости
}


public enum CookingRequired
{
    ГотовыйКУпотреблению,
    Полуфабрикат,
    ТребуетПриготовления
}


[Flags]
public enum ProductFlags
{
    None = 0,
    Веган = 1,
    БезГлютена = 2,
    БезСахара = 4
}


public enum ProductSortParam
{
    Name,
    Calories,
    Proteins,
    Fats,
    Carbs
}


public class ProductFilter
{
    public ProductCategory? Category { get; set; }
    public CookingRequired? CookingRequired { get; set; }
    public ProductFlags? Flags { get; set; }
    public string? Search { get; set; }
    public ProductSortParam? SortBy { get; set; }
}


public class Product
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

}