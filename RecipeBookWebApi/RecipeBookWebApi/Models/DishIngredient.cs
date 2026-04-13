using System.ComponentModel.DataAnnotations;

namespace RecipeBookWebApi.Models;


public class DishIngredient
{
    public int Id { get; set; }
    public int DishId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public double Amount { get; set; }
}