using Moq;
using RecipeBookWebApi.Data;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;
using RecipeBookWebApi.Services;

namespace RecipeBookWebApi.Tests;


public class DishServiceTests
{
    private readonly RecipeBookContext _context;
    private readonly Mock<IProductService> _mockProductService;
    private readonly DishService _dishService;

    public DishServiceTests()
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

        var request = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = 1, Amount = 150 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(27, result.Calories);
        Assert.Equal(1.35, result.Proteins);
        Assert.Equal(0.3, result.Fats);
        Assert.Equal(5.85, result.Carbs);
    }


    [Fact(DisplayName = "Рассчет КБЖУ множества ингредиентов с положительными значениями количества")]
    public async Task CalculateNutritionAsync_MultipleIngredients_PositiveAmount_CalculatesCorrectly()
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
                new DishIngredientDto { ProductId = 2, Amount = 100 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(377.5, result.Calories);
        Assert.Equal(49.2, result.Proteins);
        Assert.Equal(5.7, result.Fats);
        Assert.Equal(28, result.Carbs);
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


    [Fact(DisplayName = "Рассчет КБЖУ множества ингредиентов с отрицательными и положительными значениями количества")]
    public async Task CalculateNutritionAsync_MultipleIngredients_NegativeAndPositiveAmount_ThrowsArgumentException()
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


    [Fact(DisplayName = "Рассчет КБЖУ для одного ингредиента c около нулевым положительным значением количества")]
    public async Task CalculateNutritionAsync_RightZeroAmount_CalculatesCorrectly()
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
                new DishIngredientDto { ProductId = 1, Amount = 0.1 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(0.018, result.Calories, 3);
        Assert.Equal(0.0009, result.Proteins, 4);
        Assert.Equal(0.0002, result.Fats, 4);
        Assert.Equal(0.0039, result.Carbs, 4);
    }

    [Fact(DisplayName = "Рассчет КБЖУ для одного ингредиента c около нулевым отрицательным значением количества")]
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


    [Fact(DisplayName = "Рассчет КБЖУ для несуществующих ингредиентов")]
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

    [Fact(DisplayName = "Рассчет КБЖУ для ингредиента с дробным положительным значением количества")]
    public async Task CalculateNutritionAsync_SingleIngredient_FloatPositiveAmounts_CalculatesCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Sugar",
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
                new DishIngredientDto { ProductId = 1, Amount = 25.5 }
            }
        };

        // Act
        var result = await _dishService.CalculateNutritionAsync(request);

        // Assert
        Assert.Equal(98.685, result.Calories, 3);
        Assert.Equal(0, result.Proteins);
        Assert.Equal(0, result.Fats);
        Assert.Equal(25.5, result.Carbs);
    }
}