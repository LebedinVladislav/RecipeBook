using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeBookWebApi.Dto;
using RecipeBookWebApi.Models;

namespace RecipeBookWebApi.ApiTests.Tests;

public partial class DishApiTests : IClassFixture<ApiFixture>
{
    private readonly ProductApiClient _productApi;
    private readonly DishApiClient _dishApi;
    private readonly ApiFixture _fixture;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DishApiTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _productApi = new ProductApiClient(fixture.Client);
        _dishApi = new DishApiClient(fixture.Client);
    }


    public static TheoryData<List<Action<ProductRequestDto>>, Action<DishRequestDto>> ValidDishes()
    {
        return new TheoryData<List<Action<ProductRequestDto>>, Action<DishRequestDto>>
        {
            {
                new List<Action<ProductRequestDto>>
                {
                    product =>
                    {
                        product.Name = "green lentils";
                        product.Calories = 116;
                        product.Proteins = 9;
                        product.Fats = 1;
                        product.Carbs = 20;
                        product.Category = ProductCategory.Крупы;
                        product.Flags = ProductFlags.Веган | ProductFlags.БезГлютена;
                    },
                    product =>
                    {
                        product.Name = "quinoa mix";
                        product.Calories = 120;
                        product.Proteins = 4;
                        product.Fats = 2;
                        product.Carbs = 18;
                        product.Category = ProductCategory.Крупы;
                        product.Flags = ProductFlags.Веган | ProductFlags.БезГлютена;
                    }
                },
                dish =>
                {
                    dish.Name = "My healthy dish";
                    dish.Photos = new List<string> { "https://example.com/dish.jpg" };
                    dish.Calories = 220;
                    dish.Proteins = 13;
                    dish.Fats = 3;
                    dish.Carbs = 38;
                    dish.PortionSize = 180;
                    dish.Category = DishCategory.Второе;
                    dish.Flags = DishFlags.Веган | DishFlags.БезГлютена;
                }
            },
            {
                new List<Action<ProductRequestDto>>
                {
                    product =>
                    {
                        product.Name = "almond milk";
                        product.Calories = 30;
                        product.Proteins = 1;
                        product.Fats = 2;
                        product.Carbs = 1;
                        product.Category = ProductCategory.Жидкость;
                        product.Flags = ProductFlags.БезСахара;
                    },
                    product =>
                    {
                        product.Name = "berry mix";
                        product.Calories = 50;
                        product.Proteins = 1;
                        product.Fats = 0;
                        product.Carbs = 8;
                        product.Category = ProductCategory.Овощи;
                        product.Flags = ProductFlags.БезСахара;
                    }
                },
                dish =>
                {
                    dish.Name = "Light dessert";
                    dish.Calories = 80;
                    dish.Proteins = 2;
                    dish.Fats = 2;
                    dish.Carbs = 9;
                    dish.PortionSize = 120;
                    dish.Category = DishCategory.Десерт;
                    dish.Flags = DishFlags.БезСахара;
                }
            },
            {
                new List<Action<ProductRequestDto>>
                {
                    product =>
                    {
                        product.Name = "chicken breast";
                        product.Calories = 110;
                        product.Proteins = 23;
                        product.Fats = 2;
                        product.Carbs = 0;
                        product.Category = ProductCategory.Мясной;
                        product.CookingRequired = CookingRequired.ТребуетПриготовления;
                        product.Flags = ProductFlags.None;
                    }
                },
                dish =>
                {
                    dish.Name = "Simple salad";
                    dish.Calories = 120;
                    dish.Proteins = 23;
                    dish.Fats = 2;
                    dish.Carbs = 5;
                    dish.PortionSize = 150;
                    dish.Category = DishCategory.Салат;
                    dish.Flags = DishFlags.None;
                }
            },
            {
                new List<Action<ProductRequestDto>>
                {
                    product =>
                    {
                        product.Name = "broccoli";
                        product.Calories = 34;
                        product.Proteins = 3;
                        product.Fats = 0;
                        product.Carbs = 7;
                        product.Category = ProductCategory.Овощи;
                        product.Flags = ProductFlags.Веган | ProductFlags.БезГлютена;
                    },
                    product =>
                    {
                        product.Name = "rice";
                        product.Calories = 130;
                        product.Proteins = 2;
                        product.Fats = 1;
                        product.Carbs = 28;
                        product.Category = ProductCategory.Крупы;
                        product.Flags = ProductFlags.БезГлютена;
                    },
                    product =>
                    {
                        product.Name = "cheese";
                        product.Calories = 90;
                        product.Proteins = 6;
                        product.Fats = 7;
                        product.Carbs = 1;
                        product.Category = ProductCategory.Мясной;
                        product.CookingRequired = CookingRequired.ГотовыйКУпотреблению;
                        product.Flags = ProductFlags.None;
                    }
                },
                dish =>
                {
                    dish.Name = "Mixed dinner";
                    dish.Calories = 254;
                    dish.Proteins = 11;
                    dish.Fats = 8;
                    dish.Carbs = 36;
                    dish.PortionSize = 220;
                    dish.Category = DishCategory.Второе;
                    dish.Flags = DishFlags.None;
                }
            }
        };
    }


    [Fact(DisplayName = "Получение всех блюд")]
    public async Task GetAllDishes_ReturnsOK()
    {
        var response = await _dishApi.GetAllDishes();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Theory(DisplayName = "Создание блюда с валидными данными")]
    [MemberData(nameof(ValidDishes))]
    public async Task CreateDish_ReturnsCreatedDish(List<Action<ProductRequestDto>> productCustomizers, Action<DishRequestDto> customizeDish)
    {
        var products = new List<ProductResponseDto>();
        foreach (var customizeProduct in productCustomizers)
        {
            products.Add(await CreateProductAsync(NewProductRequest(customizeProduct)));
        }

        var request = NewDishRequest(dish =>
        {
            foreach (var product in products)
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = product.Id, Amount = 100 });
            }

            customizeDish(dish);
        });

        var createdDish = await CreateDishAsync(request);
        AssertDishMatchesRequest(request, createdDish);
    }


    [Theory(DisplayName = "Получение блюда по id")]
    [MemberData(nameof(ValidDishes))]
    public async Task GetDishById_ReturnsDish(List<Action<ProductRequestDto>> productCustomizers, Action<DishRequestDto> customizeDish)
    {
        var products = new List<ProductResponseDto>();
        foreach (var customizeProduct in productCustomizers)
        {
            products.Add(await CreateProductAsync(NewProductRequest(customizeProduct)));
        }

        var request = NewDishRequest(dish =>
        {
            foreach (var product in products)
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = product.Id, Amount = 100 });
            }

            customizeDish(dish);
        });

        var createdDish = await CreateDishAsync(request);
        var response = await _dishApi.GetDish(createdDish.Id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
       
        var responseDish = await response.Content.ReadFromJsonAsync<DishResponseDto>(JsonOptions);
        Assert.NotNull(responseDish);
        AssertDishMatchesRequest(request, responseDish!);
    }


    [Theory(DisplayName = "Фильтрация блюд")]
    [MemberData(nameof(DishFilterCases))]
    public async Task GetDishes_Filter_ReturnsExpectedCount(string query, int expectedCount)
    {
        var regularProduct = await CreateProductAsync(NewProductRequest(product =>
        {
            product.Name = "filter-regular-product";
            product.Flags = ProductFlags.None;
        }));
        var noSugarProduct = await CreateProductAsync(NewProductRequest(product =>
        {
            product.Name = "filter-no-sugar-product";
            product.Flags = ProductFlags.БезСахара;
        }));
        
        var dish1 = await CreateDishAsync(NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = regularProduct.Id, Amount = 100 });
            dish.Name = "filter-category-soup";
            dish.Category = DishCategory.Суп;
        }), addToFixture: false);
        
        var dish2 = await CreateDishAsync(NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = regularProduct.Id, Amount = 100 });
            dish.Name = "filter-other-dessert";
            dish.Category = DishCategory.Десерт;
        }), addToFixture: false);
        
        var dish3 = await CreateDishAsync(NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = noSugarProduct.Id, Amount = 100 });
            dish.Name = "filter-sugar-free-snack";
            dish.Category = DishCategory.Перекус;
            dish.Flags = DishFlags.БезСахара;
        }), addToFixture: false);
        
        var dish4 = await CreateDishAsync(NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = regularProduct.Id, Amount = 100 });
            dish.Name = "filter-gluten-free-salad";
            dish.Category = DishCategory.Салат;
            dish.Flags = DishFlags.БезГлютена;
        }), addToFixture: false);

        var dishes = await GetDishesAsync(query);
        Assert.Equal(expectedCount, dishes.Count);
        
        // Cleanup
        await _dishApi.DeleteDish(dish1.Id);
        await _dishApi.DeleteDish(dish2.Id);
        await _dishApi.DeleteDish(dish3.Id);
        await _dishApi.DeleteDish(dish4.Id);
    }

    public static TheoryData<string, int> DishFilterCases()
    {
        return new TheoryData<string, int>
        {
            { $"category={(int)DishCategory.Суп}&search=filter-category-soup", 1 },
            { $"flags={(int)DishFlags.БезСахара}&search=filter-sugar-free-snack", 1 }
        };
    }


    [Theory(DisplayName = "Макрос в названии блюда назначает категорию")]
    [MemberData(nameof(MacroCases))]
    public async Task CreateDish_WithMacroInName_AssignsCategory(string name, DishCategory? explicitCategory, DishCategory expectedCategory)
    {
        var product = await CreateProductAsync(NewProductRequest(product => product.Name = "macro-product"));
        var request = NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = product.Id, Amount = 100 });
            dish.Name = name;
            dish.Category = explicitCategory;
        });

        var createdDish = await CreateDishAsync(request);
        Assert.Equal(expectedCategory, createdDish.Category);
        Assert.Equal(name, createdDish.Name);
    }

    public static TheoryData<string, DishCategory?, DishCategory> MacroCases()
    {
        return new TheoryData<string, DishCategory?, DishCategory>
        {
            { "!десерт sweet treat", null, DishCategory.Десерт },
            { "!десерт explicit override", DishCategory.Второе, DishCategory.Второе },
            { "!первое !второе lunch", null, DishCategory.Первое },
            { "!первое !второе lunch", DishCategory.Салат, DishCategory.Салат }
        };
    }


    [Theory(DisplayName = "Создание блюда с невалидными данными возвращает ошибку")]
    [MemberData(nameof(InvalidDishCases))]
    public async Task CreateDish_InvalidData_ReturnsBadRequest(Func<int, DishRequestDto> createInvalidRequest)
    {
        var product = await CreateProductAsync(NewProductRequest(product => product.Name = "invalid-dish-product"));
        var request = createInvalidRequest(product.Id);

        var response = await _dishApi.CreateDish(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public static TheoryData<Func<int, DishRequestDto>> InvalidDishCases()
    {
        return new TheoryData<Func<int, DishRequestDto>>
        {
            productId => NewDishRequest(dish =>
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = productId, Amount = 100 });
                dish.Name = "A";
            }),
            productId => NewDishRequest(dish =>
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = productId, Amount = 100 });
                dish.Name = "no category dish";
                dish.Category = null;
            }),
            productId => NewDishRequest(dish =>
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = productId, Amount = 100 });
                dish.Name = "negative nutrition";
                dish.Calories = -10;
                dish.Proteins = -1;
                dish.Fats = -1;
                dish.Carbs = -1;
            }),
            productId => NewDishRequest(dish =>
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = productId, Amount = 100 });
                dish.Ingredients[0].Amount = 0;
            }),
            productId =>
            {
                var dish = NewDishRequest(dish => dish.Ingredients.Add(new DishIngredientDto { ProductId = productId, Amount = 100 }));
                dish.Ingredients.Clear();
                return dish;
            }
        };
    }


    [Fact(DisplayName = "Вычисление КБЖУ блюда по составу возвращает корректные значения")]
    public async Task CalculateNutrition_ReturnsExpectedValues()
    {
        var product1 = await CreateProductAsync(NewProductRequest(product => 
        {
            product.Name = "calc-product-1";
            product.Calories = 160;
            product.Proteins = 20;
            product.Fats = 8;
            product.Carbs = 32;
        }));
        var product2 = await CreateProductAsync(NewProductRequest(product => 
        {
            product.Name = "calc-product-2";
            product.Calories = 80;
            product.Proteins = 0;
            product.Fats = 4;
            product.Carbs = 6;
        }));

        var calculateRequest = new NutritionCalculateRequestDto
        {
            Ingredients = new List<DishIngredientDto>
            {
                new DishIngredientDto { ProductId = product1.Id, Amount = 100 },
                new DishIngredientDto { ProductId = product2.Id, Amount = 50 }
            }
        };

        var response = await _dishApi.CalculateNutrition(calculateRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var nutrition = await response.Content.ReadFromJsonAsync<NutritionResponseDto>(JsonOptions);
        Assert.NotNull(nutrition);
        Assert.Equal(200, nutrition!.Calories);
        Assert.Equal(20, nutrition.Proteins);
        Assert.Equal(10, nutrition.Fats);
        Assert.Equal(35, nutrition.Carbs);
    }


    [Fact(DisplayName = "Обновление блюда изменяет состав и категорию")]
    public async Task UpdateDish_ChangesIngredientsAndCategory()
    {
        var firstProduct = await CreateProductAsync(NewProductRequest(product => product.Name = "update-dish-product-1"));
        var secondProduct = await CreateProductAsync(NewProductRequest(product => product.Name = "update-dish-product-2"));

        var createdDish = await CreateDishAsync(NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = firstProduct.Id, Amount = 100 });
            dish.Name = "initial dish";
            dish.Calories = 130;
            dish.Proteins = 8;
            dish.Fats = 4;
            dish.Carbs = 12;
            dish.Category = DishCategory.Второе;
        }));

        var updateRequest = NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = secondProduct.Id, Amount = 100 });
            dish.Name = "!салат updated dish";
            dish.Category = DishCategory.Салат;
            dish.Calories = 240;
            dish.Proteins = 15;
            dish.Fats = 9;
            dish.Carbs = 25;
            dish.PortionSize = 140;
        });

        var updateResponse = await UpdateDishAsync(createdDish.Id, updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _dishApi.GetDish(createdDish.Id);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updatedDish = await getResponse.Content.ReadFromJsonAsync<DishResponseDto>(JsonOptions);
        Assert.NotNull(updatedDish);
        Assert.Equal(updateRequest.Name, updatedDish!.Name);
        Assert.Equal(DishCategory.Салат, updatedDish.Category);
        Assert.Single(updatedDish.Ingredients);
        Assert.Equal(secondProduct.Id, updatedDish.Ingredients[0].ProductId);
        Assert.Equal(updateRequest.PortionSize, updatedDish.PortionSize);
    }


    [Theory(DisplayName = "Удаление блюда по id возвращает NoContent")]
    [MemberData(nameof(ValidDishes))]
    public async Task DeleteDishById_ReturnsNoContent(List<Action<ProductRequestDto>> productCustomizers, Action<DishRequestDto> customizeDish)
    {
        var products = new List<ProductResponseDto>();
        foreach (var customizeProduct in productCustomizers)
        {
            products.Add(await CreateProductAsync(NewProductRequest(customizeProduct)));
        }

        var request = NewDishRequest(dish =>
        {
            foreach (var product in products)
            {
                dish.Ingredients.Add(new DishIngredientDto { ProductId = product.Id, Amount = 100 });
            }

            customizeDish(dish);
        });
        var createdDish = await CreateDishAsync(request);

        var response = await _dishApi.DeleteDish(createdDish.Id);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }


    [Fact(DisplayName = "Получение несуществующего блюда возвращает 404")]
    public async Task GetDish_NonExisting_ReturnsNotFound()
    {
        var response = await _dishApi.GetDish(int.MaxValue);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact(DisplayName = "Обновление несуществующего блюда возвращает 404")]
    public async Task UpdateDish_NonExisting_ReturnsNotFound()
    {
        var request = NewDishRequest(dish =>
        {
            dish.Ingredients.Add(new DishIngredientDto { ProductId = int.MaxValue, Amount = 100 });
            dish.Name = "not found";
            dish.Category = DishCategory.Суп;
        });

        var response = await UpdateDishAsync(int.MaxValue, request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    private async Task<ProductResponseDto> CreateProductAsync(ProductRequestDto request)
    {
        var response = await _productApi.CreateProductAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ProductResponseDto? createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions);
        Assert.NotNull(createdProduct);

        _fixture.AddCreatedProduct(createdProduct);
        return createdProduct!;
    }

    private static ProductRequestDto NewProductRequest(Action<ProductRequestDto>? customize = null)
    {
        var request = new ProductRequestDto
        {
            Name = "default product",
            Calories = 100,
            Proteins = 5,
            Fats = 5,
            Carbs = 10,
            Category = ProductCategory.Крупы,
            CookingRequired = CookingRequired.ГотовыйКУпотреблению,
            Flags = ProductFlags.None
        };

        customize?.Invoke(request);
        return request;
    }

    private static DishRequestDto NewDishRequest(Action<DishRequestDto>? customize = null)
    {
        var request = new DishRequestDto
        {
            Name = "default dish",
            Photos = new List<string>(),
            Calories = 100,
            Proteins = 5,
            Fats = 5,
            Carbs = 10,
            Ingredients = new List<DishIngredientDto>(),
            PortionSize = 100,
            Category = DishCategory.Второе,
            Flags = DishFlags.None
        };

        customize?.Invoke(request);
        return request;
    }

    private async Task<DishResponseDto> CreateDishAsync(DishRequestDto request, bool addToFixture = true)
    {
        var response = await _dishApi.CreateDish(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        DishResponseDto? createdDish = await response.Content.ReadFromJsonAsync<DishResponseDto>(JsonOptions);
        Assert.NotNull(createdDish);

        if (addToFixture)
        {
            _fixture.AddCreatedDish(createdDish);
        }
        return createdDish!;
    }

    private async Task<List<DishResponseDto>> GetDishesAsync(string query = "")
    {
        var response = await _dishApi.GetAllDishes(query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<List<DishResponseDto>>(JsonOptions) ?? new List<DishResponseDto>();
    }

    private async Task<HttpResponseMessage> UpdateDishAsync(int id, DishRequestDto request)
    {
        return await _dishApi.UpdateDish(id, request);
    }

    private void AssertDishMatchesRequest(DishRequestDto request, DishResponseDto actual)
    {
        Assert.Equal(request.Name, actual.Name);
        Assert.Equal(request.Photos, actual.Photos);
        Assert.Equal(request.Calories, actual.Calories);
        Assert.Equal(request.Proteins, actual.Proteins);
        Assert.Equal(request.Fats, actual.Fats);
        Assert.Equal(request.Carbs, actual.Carbs);
        Assert.Equal(request.PortionSize, actual.PortionSize);
        Assert.Equal(request.Category, actual.Category);
        Assert.Equal(request.Flags, actual.Flags);
        Assert.Equal(request.Ingredients.Count, actual.Ingredients.Count);
        Assert.Equal(request.Ingredients.Select(i => (i.ProductId, i.Amount)), actual.Ingredients.Select(i => (i.ProductId, i.Amount)));
    }
}

