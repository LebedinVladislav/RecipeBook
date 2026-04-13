namespace RecipeBookWebApi.Dto;

/// <summary>
/// Рассчитанные значения КБЖУ на порцию
/// </summary>
public class NutritionResponseDto
{
    /// <summary>
    /// Калории на порцию (ккал)
    /// </summary>
    public double Calories { get; set; }

    /// <summary>
    /// Белки на порцию (г)
    /// </summary>
    public double Proteins { get; set; }

    /// <summary>
    /// Жиры на порцию (г)
    /// </summary>
    public double Fats { get; set; }

    /// <summary>
    /// Углеводы на порцию (г)
    /// </summary>
    public double Carbs { get; set; }
}
