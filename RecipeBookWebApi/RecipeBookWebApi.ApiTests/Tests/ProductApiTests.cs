using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeBookWebApi.ApiTests.ClassData;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.ApiTests.Tests;


public partial class ProductApiTests : IClassFixture<ApiFixture>
{
    private readonly ProductApiClient _productApi;
    private readonly DishApiClient _dishApi;
    private static readonly List<ProductResponseDto> _createdProducts = [];
    private static readonly List<DishResponseDto> _createdDishes = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    internal static async Task DeleteCreatedEntities(HttpClient client)
    {
        ProductApiClient productApi = new ProductApiClient(client);
        DishApiClient dishApi = new DishApiClient(client);
        
        foreach(var product in _createdProducts)
        {
            await productApi.DeleteProduct(product.Id);
        }

        foreach(var dish in _createdDishes)
        {
            await dishApi.DeleteDish(dish.Id);
        }
    }

    private void IsProductsEqual(ProductResponseDto product1, ProductResponseDto product2)
    {
        Assert.Equal(product1.Name, product2.Name);
        Assert.Equal(product1.Photos, product2.Photos);
        Assert.Equal(product1.Calories, product2.Calories);
        Assert.Equal(product1.Proteins, product2.Proteins);
        Assert.Equal(product1.Fats, product2.Fats);
        Assert.Equal(product1.Carbs, product2.Carbs);
        Assert.Equal(product1.Composition, product2.Composition);
        Assert.Equal(product1.Category, product2.Category);
        Assert.Equal(product1.CookingRequired, product2.CookingRequired);
        Assert.Equal(product1.Flags, product2.Flags);
    }

    public ProductApiTests(ApiFixture fixture)
    {
        _productApi = new ProductApiClient(fixture.Client);
        _dishApi = new DishApiClient(fixture.Client);
    }

    private async Task<ProductResponseDto> CreateProductAsync(ProductRequestDto request)
    {
        var response = await _productApi.CreateProduct(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ProductResponseDto? createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions);
        Assert.NotNull(createdProduct);

        _createdProducts.Add(createdProduct);
        return createdProduct;
    }

    private async Task<DishResponseDto> CreateDishAsync(DishRequestDto request)
    {
        var response = await _dishApi.CreateDish(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        DishResponseDto? createdDish = await response.Content.ReadFromJsonAsync<DishResponseDto>(JsonOptions);
        Assert.NotNull(createdDish);

        _createdDishes.Add(createdDish);
        return createdDish;
    }

    private async Task<List<ProductResponseDto>> GetProductsAsync(string query)
    {
        var response = await _productApi.GetProducts(query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<List<ProductResponseDto>>(JsonOptions) ?? new List<ProductResponseDto>();
    }

    [Fact(DisplayName = "Получение всех продуктов")]
    public async Task GetAllProducts_ReturnsOK()
    {
        var response = await _productApi.GetAllProducts();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Theory(DisplayName = "Создание продукта с валидными данными")]
    [ClassData(typeof(ValidProductTestData))]
    public async Task CreateProduct_ReturnsProduct(ProductRequestDto requestProduct)
    {
        var response = await _productApi.CreateProduct(requestProduct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        ProductResponseDto? responseProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions);
        Assert.NotNull(responseProduct);
        

        _createdProducts.Add(responseProduct);
    }


    [Theory(DisplayName = "Получение существующего продукта по id")]
    [ClassData(typeof(ValidProductTestData))]
    public async Task GetProductById_ReturnsProduct(ProductRequestDto product)
    {
        var response = await _productApi.CreateProduct(product);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ProductResponseDto? createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions);
        Assert.NotNull(createdProduct);
        
        response = await _productApi.GetProduct(createdProduct.Id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        ProductResponseDto? responseProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions);
        Assert.NotNull(responseProduct);
        IsProductsEqual(responseProduct, createdProduct);
    }


    [Theory(DisplayName = "Удаление существующего продукта по id, который не состоит в блюдах")]
    [ClassData(typeof(ValidProductTestData))]
    public async Task DeleteProductById_ReturnsProduct(ProductRequestDto requestProduct)
    {
        var response = await _productApi.CreateProduct(requestProduct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ProductResponseDto? createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions);
        Assert.NotNull(createdProduct);

        response = await _productApi.DeleteProduct(createdProduct.Id);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "Фильтрация продуктов по категории")]
    public async Task GetProducts_FilterByCategory_ReturnsFilteredProducts()
    {
        var expectedProduct = await CreateProductAsync(new ProductRequestDto
        {
            Name = "filter-category-product",
            Calories = 100,
            Proteins = 10,
            Fats = 10,
            Carbs = 10,
            Category = ProductCategory.Крупы,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        });

        await CreateProductAsync(new ProductRequestDto
        {
            Name = "other-category-product",
            Calories = 120,
            Proteins = 10,
            Fats = 10,
            Carbs = 10,
            Category = ProductCategory.Жидкость,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        });

        var products = await GetProductsAsync($"category={(int)ProductCategory.Крупы}&search=filter-category-product");

        Assert.Single(products);
        Assert.Equal(expectedProduct.Id, products[0].Id);
        Assert.Equal(ProductCategory.Крупы, products[0].Category);
    }

    [Fact(DisplayName = "Фильтрация продуктов по требованию приготовления")]
    public async Task GetProducts_FilterByCookingRequired_ReturnsFilteredProducts()
    {
        var expectedProduct = await CreateProductAsync(new ProductRequestDto
        {
            Name = "filter-cooking-required-product",
            Calories = 80,
            Proteins = 5,
            Fats = 5,
            Carbs = 20,
            Category = ProductCategory.Жидкость,
            CookingRequired = CookingRequired.ТребуетПриготовления
        });

        await CreateProductAsync(new ProductRequestDto
        {
            Name = "other-cooking-required-product",
            Calories = 90,
            Proteins = 4,
            Fats = 4,
            Carbs = 22,
            Category = ProductCategory.Жидкость,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        });

        var products = await GetProductsAsync($"cookingRequired={(int)CookingRequired.ТребуетПриготовления}&search=filter-cooking-required-product");

        Assert.Single(products);
        Assert.Equal(expectedProduct.Id, products[0].Id);
        Assert.Equal(CookingRequired.ТребуетПриготовления, products[0].CookingRequired);
    }

    [Fact(DisplayName = "Фильтрация продуктов по флагам")]
    public async Task GetProducts_FilterByFlags_ReturnsFilteredProducts()
    {
        var expectedProduct = await CreateProductAsync(new ProductRequestDto
        {
            Name = "filter-flags-product",
            Calories = 200,
            Proteins = 15,
            Fats = 10,
            Carbs = 20,
            Category = ProductCategory.Жидкость,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению,
            Flags = ProductFlags.Веган | ProductFlags.БезСахара
        });

        await CreateProductAsync(new ProductRequestDto
        {
            Name = "other-flags-product",
            Calories = 210,
            Proteins = 15,
            Fats = 10,
            Carbs = 20,
            Category = ProductCategory.Жидкость,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению,
            Flags = ProductFlags.БезГлютена
        });

        var products = await GetProductsAsync($"flags={(int)ProductFlags.Веган}&search=filter-flags-product");

        Assert.Single(products);
        Assert.Equal(expectedProduct.Id, products[0].Id);
        Assert.True(products[0].Flags.HasFlag(ProductFlags.Веган));
    }

    [Fact(DisplayName = "Создание продукта с отрицательным кбжу возвращает 400")]
    public async Task CreateProduct_WithNegativeNutrition_ReturnsBadRequest()
    {
        var request = new ProductRequestDto
        {
            Name = "invalid nutrition",
            Calories = -10,
            Proteins = -1,
            Fats = -1,
            Carbs = -1,
            Category = ProductCategory.Крупы,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        };

        var response = await _productApi.CreateProduct(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Создание продукта с именем короче двух символов возвращает 400")]
    public async Task CreateProduct_WithShortName_ReturnsBadRequest()
    {
        var request = new ProductRequestDto
        {
            Name = "A",
            Calories = 10,
            Proteins = 1,
            Fats = 1,
            Carbs = 1,
            Category = ProductCategory.Крупы,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        };

        var response = await _productApi.CreateProduct(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Создание продукта без указания категории возвращает 400")]
    public async Task CreateProduct_WithoutCategory_ReturnsBadRequest()
    {
        var request = new ProductRequestDto
        {
            Name = "no category",
            Calories = 10,
            Proteins = 1,
            Fats = 1,
            Carbs = 1,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        };

        var response = await _productApi.CreateProduct(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Создание продукта без указания обязательности готовки возвращает 400")]
    public async Task CreateProduct_WithoutCookingRequired_ReturnsBadRequest()
    {
        var request = new ProductRequestDto
        {
            Name = "no cooking required",
            Calories = 10,
            Proteins = 1,
            Fats = 1,
            Carbs = 1,
            Category = ProductCategory.Крупы,
        };

        var response = await _productApi.CreateProduct(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Получение несуществующего продукта возвращает 404")]
    public async Task GetProduct_NonExisting_ReturnsNotFound()
    {
        var response = await _productApi.GetProduct(int.MaxValue);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Удаление несуществующего продукта возвращает 404")]
    public async Task DeleteProduct_NonExisting_ReturnsNotFound()
    {
        var response = await _productApi.DeleteProduct(int.MaxValue);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Удаление продукта, который используется в блюде, возвращает 400")]
    public async Task DeleteProduct_UsedInDish_ReturnsBadRequest()
    {
        var product = await CreateProductAsync(new ProductRequestDto
        {
            Name = "dish-linked-product",
            Calories = 50,
            Proteins = 5,
            Fats = 5,
            Carbs = 5,
            Category = ProductCategory.Жидкость,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению
        });

        await CreateDishAsync(new DishRequestDto
        {
            Name = "dish using product",
            Calories = 60,
            Proteins = 5,
            Fats = 5,
            Carbs = 5,
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = product.Id, Amount = 1 }
            },
            PortionSize = 20,
            Category = DishCategory.Первое
        });

        var response = await _productApi.DeleteProduct(product.Id);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

