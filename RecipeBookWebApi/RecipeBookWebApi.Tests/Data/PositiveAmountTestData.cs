using System.Collections;
using RecipeBookWebApi.Models;

public class PositiveAmountTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        
        yield return new object[]
        {
            new List<DishIngredient>()
            {
                new DishIngredient()
                {
                    Product = new ProductTest(1, 18, 0.9, 0.2, 3.9),
                    Amount = 150
                }
            },
            27, 1.4, 0.3, 5.9
        };
        
        yield return new object[]
        {
            new List<DishIngredient>()
            {
                new DishIngredient()
                {
                    Product = new ProductTest(1, 165, 31, 3.6, 0),
                    Amount = 50
                },
                new DishIngredient()
                {
                    Product = new ProductTest(2, 130, 2.7, 0.3, 28),
                    Amount = 200
                }
            },
            342.5, 20.9, 2.4, 56
        };
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}