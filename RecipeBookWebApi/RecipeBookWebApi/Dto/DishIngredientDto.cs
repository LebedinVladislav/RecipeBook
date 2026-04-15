using System.ComponentModel.DataAnnotations;

namespace RecipeBookWebApi.Dto;

public class DishIngredientDto
{
    public int ProductId { get; set; }
    public double Amount { get; set; }
}