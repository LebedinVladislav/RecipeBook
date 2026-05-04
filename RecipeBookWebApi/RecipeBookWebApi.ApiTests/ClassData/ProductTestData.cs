using System.Collections;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.ApiTests.ClassData;

public class ValidProductTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "zero",
                Calories = 0,
                Proteins = 0,
                Fats = 0,
                Carbs = 0,
                Category = ProductCategory.Крупы,
                CookingRequired = CookingRequired.ТребуетПриготовления,
            }
        };
        
        // Граничные значения: минимальные положительные числа      
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "min positive",
                Calories = 0.01,
                Proteins = 0.01,
                Fats = 0.01,
                Carbs = 0.01,
                Category = ProductCategory.Крупы,
                CookingRequired = CookingRequired.ТребуетПриготовления,
            }
        };

        // Граничные значения: высокие значения
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "high values",
                Calories = 1000,
                Proteins = 33.33,
                Fats = 33.33,
                Carbs = 33.33,
                Category = ProductCategory.Жидкость,
                CookingRequired = CookingRequired.ГотовыйКУпотреблению,
                Flags = ProductFlags.БезГлютена
            }
        };

        // Эквивалентное разбиение: разные категории
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "fruit product",
                Calories = 50,
                Proteins = 1,
                Fats = 0.5,
                Carbs = 12,
                Category = ProductCategory.Жидкость,
                CookingRequired = CookingRequired.ГотовыйКУпотреблению,
                Flags = ProductFlags.Веган | ProductFlags.БезСахара
            }
        };

        // Эквивалентное разбиение: разные CookingRequired
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "cooked product",
                Calories = 200,
                Proteins = 20,
                Fats = 15,
                Carbs = 10,
                Category = ProductCategory.Крупы,
                CookingRequired = CookingRequired.ТребуетПриготовления,
            }
        };

        // Граничные значения: имя с пробелами
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "product with spaces",
                Calories = 150,
                Proteins = 5,
                Fats = 3,
                Carbs = 20,
                Category = ProductCategory.Жидкость, 
                CookingRequired = CookingRequired.ГотовыйКУпотреблению,
                Flags = ProductFlags.Веган
            }
        };

        // Эквивалентное разбиение: все флаги
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "all flags",
                Calories = 300,
                Proteins = 25,
                Fats = 20,
                Carbs = 30,
                Category = ProductCategory.Крупы,
                CookingRequired = CookingRequired.ТребуетПриготовления,
                Flags = ProductFlags.Веган | ProductFlags.БезГлютена | ProductFlags.БезСахара
            }
        };

        // Граничные значения: длинное имя
        yield return new object[]
        {
            new ProductRequestDto()
            {
                Name = "very long product name that exceeds normal length to test boundary conditions in the system",
                Calories = 100,
                Proteins = 10,
                Fats = 5,
                Carbs = 15,
                Category = ProductCategory.Жидкость,
                CookingRequired = CookingRequired.ГотовыйКУпотреблению,
            }
        };
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}