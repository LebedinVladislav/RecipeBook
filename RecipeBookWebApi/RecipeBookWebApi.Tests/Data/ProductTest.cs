using System.Data.Common;
using RecipeBookWebApi.Models;

public class ProductTest : Product
{
    public ProductTest()
    {
    }

    public ProductTest(int id, double cals, double prots, double fats, double carbs)
    {
        Id = id;
        Calories = cals;
        Proteins = prots;
        Fats = fats;
        Carbs = carbs;
    }
}