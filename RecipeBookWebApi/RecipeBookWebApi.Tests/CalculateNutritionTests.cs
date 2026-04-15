using Moq;
using RecipeBookWebApi.Data;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Migrations;
using RecipeBookWebApi.Models;
using RecipeBookWebApi.Services;

namespace RecipeBookWebApi.Tests;


public class CalculateNutritionTests
{
    private readonly RecipeBookContext _context;
    private readonly Mock<IProductService> _mockProductService;
    private readonly DishService _dishService;

    public CalculateNutritionTests()
    {
        _context = new RecipeBookContext();
        _mockProductService = new Mock<IProductService>();
        _dishService = new DishService(_context, _mockProductService.Object);
    }


    [Fact(DisplayName = "Рассчет КБЖУ одного ингредиента с положительным значением количества")]
    public async Task CalculateNutritionAsync_SingleIngredient_PositiveAmount_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Tomato",
            Calories = 18,
            Proteins = 0.9,
            Fats = 0.2,
            Carbs = 3.9
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product);

        var data = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 150 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(data);

        // Assert
        Assert.Equal(27, result.Calories); // 18 * 1.5
        Assert.Equal(1.4, result.Proteins); // 0.9 * 1.5
        Assert.Equal(0.3, result.Fats); // 0.2 * 1.5
        Assert.Equal(5.9, result.Carbs); // 3.9 * 1.5
    }


    [Fact(DisplayName = "Рассчет КБЖУ нескольких ингредиентов с положительными значениями количества")]
    public async Task CalculateNutritionAsync_ManyIngredients_PositiveAmount_CalculatesCorrectly()
    {
        // Arrange
        var product1 = new Product
        {
            Id = 1,
            Name = "Chicken Breast",
            Calories = 165,
            Proteins = 31,
            Fats = 3.6,
            Carbs = 0
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "Rice",
            Calories = 130,
            Proteins = 2.7,
            Fats = 0.3,
            Carbs = 28
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product1);
        _mockProductService.Setup(ps => ps.GetProductByIdAsync(2)).ReturnsAsync(product2);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 50 },
                new DishIngredientDto { ProductId = 2, Amount = 200 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(342.5, result.Calories); // 165 * 0.5 + 130 * 2
        Assert.Equal(20.9, result.Proteins); // 31 * 0.5 + 2.7 * 2
        Assert.Equal(2.4, result.Fats); // 3.6 * 0.5 + 0.3 * 2
        Assert.Equal(56, result.Carbs); // 28 * 2
    }


    [Fact(DisplayName = "Рассчет КБЖУ одного ингредиента с отрицательным значением количества")]
    public async Task CalculateNutritionAsync_SingleIngredient_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Tomato",
            Calories = 18,
            Proteins = 0.9,
            Fats = 0.2,
            Carbs = 3.9
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = -60 }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dishService.CalculateNutritionAsync(request));
        Assert.Contains("Ingredient quantity must be positive", exception.Message);
    }


    [Fact(DisplayName = "Рассчет КБЖУ нескольких ингредиентов с отрицательными и положительными значениями количества")]
    public async Task CalculateNutritionAsync_ManyIngredients_NegativeAndPositiveAmount_ThrowsArgumentException()
    {
        // Arrange
        var product1 = new Product
        {
            Id = 1,
            Name = "Chicken Breast",
            Calories = 165,
            Proteins = 31,
            Fats = 3.6,
            Carbs = 0
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "Rice",
            Calories = 130,
            Proteins = 2.7,
            Fats = 0.3,
            Carbs = 28
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product1);
        _mockProductService.Setup(ps => ps.GetProductByIdAsync(2)).ReturnsAsync(product2);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 150 },
                new DishIngredientDto { ProductId = 2, Amount = -100 }
            }
        };

        //Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dishService.CalculateNutritionAsync(request));
        Assert.Contains("Ingredient quantity must be positive", exception.Message);
    }
    

    [Fact(DisplayName = "Рассчет КБЖУ для одного ингредиента с нулевым значением количества")]
    public async Task CalculateNutritionAsync_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Tomato",
            Calories = 18,
            Proteins = 0.9,
            Fats = 0.2,
            Carbs = 3.9
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 0 }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dishService.CalculateNutritionAsync(request));
        Assert.Contains("Ingredient quantity must be positive", exception.Message);
    }


    [Fact(DisplayName = "Рассчет КБЖУ для одного ингредиента c околонулевым положительным значением количества")]
    public async Task CalculateNutritionAsync_RightZeroAmount_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Tomato",
            Calories = 345,
            Proteins = 0.9,
            Fats = 27,
            Carbs = 3.9
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 0.1 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(0.3, result.Calories); // 345 * 0.1 / 100
        Assert.Equal(0, result.Proteins); // 0.9 * 0.1 / 100
        Assert.Equal(0, result.Fats); // 27 * 0.1 / 100
        Assert.Equal(0, result.Carbs); // 3.9 * 0.1 / 100
    }

    [Fact(DisplayName = "Рассчет КБЖУ для одного ингредиента c околонулевым отрицательным значением количества")]
    public async Task CalculateNutritionAsync_LeftZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Tomato",
            Calories = 18,
            Proteins = 0.9,
            Fats = 0.2,
            Carbs = 3.9
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = -0.1 }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dishService.CalculateNutritionAsync(request));
        Assert.Contains("Ingredient quantity must be positive", exception.Message);
    }
    
    [Fact(DisplayName = "Рассчет КБЖУ для пустого массива ингредиентов")]
    public async Task CalculateNutritionAsync_NoIngredients_ThrowsArgumentException()
    {
        // Arrange
        var request = new NutritionCalculateRequestDto
        {
            Ingredients = []
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dishService.CalculateNutritionAsync(request));
        Assert.Contains("At least one ingredient is required", exception.Message);
    }


    [Fact(DisplayName = "Рассчет КБЖУ для несуществующего ингредиента")]
    public async Task CalculateNutritionAsync_ProductNotFound_ThrowsArgumentException()
    {
        // Arrange
        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).Throws(new ArgumentException());

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 100 }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dishService.CalculateNutritionAsync(request));
        Assert.Contains("Product 1 not found", exception.Message);
    }

    [Fact(DisplayName = "Рассчет КБЖУ для одного ингредиента с дробным положительным значением количества")]
    public async Task CalculateNutritionAsync_SingleIngredient_FloatPositiveAmounts_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Calories = 387,
            Proteins = 0,
            Fats = 0,
            Carbs = 100
        };

        _mockProductService.Setup(ps => ps.GetProductByIdAsync(1)).ReturnsAsync(product);

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 75.32 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(291.5, result.Calories); // 387 * 75.32 / 100
        Assert.Equal(0, result.Proteins); // 
        Assert.Equal(0, result.Fats); // 
        Assert.Equal(75.3, result.Carbs); // 
    }



    [Theory(DisplayName = "Тест атрибута [ClassData]")]
    [ClassData(typeof(PositiveAmountTestData))]
    public async Task CalculateNutritionAsync_MultipleIngredients_ClassDataTest(
        List<DishIngredient> ingredients, 
        double expectefCalories, 
        double expectedProteins, 
        double expectedFats, 
        double expectedCarbs
    ) {
        // Arrange

        foreach (var ingredient in ingredients)
        {
            Product product = ingredient.Product;
            _mockProductService.Setup(ps => ps.GetProductByIdAsync(product.Id)).ReturnsAsync(product);
        }

        var data = new NutritionCalculateRequestDto
        {
            Ingredients = ingredients.Select(ing => 
                new DishIngredientDto() { 
                    ProductId = ing.Product.Id, 
                    Amount = ing.Amount 
                }).ToList()
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(data);

        // Assert
        Assert.Equal(expectefCalories, result.Calories);
        Assert.Equal(expectedProteins, result.Proteins);
        Assert.Equal(expectedFats, result.Fats);
        Assert.Equal(expectedCarbs, result.Carbs);
    }
}