using Microsoft.EntityFrameworkCore;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.Data;

public class RecipeBookContext : DbContext
{
    public RecipeBookContext() {}
    public RecipeBookContext(DbContextOptions<RecipeBookContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Dish> Dishes { get; set; } = null!;
    public DbSet<DishIngredient> DishIngredients { get; set; } = null!;
}