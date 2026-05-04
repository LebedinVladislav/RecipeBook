using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeBookWebApi.Dto;

namespace RecipeBookWebApi.ApiTests.Tests;


public partial class DishApiTests : IClassFixture<ApiFixture>
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
            await productApi.DeleteProductByIdAsync(product.Id);
        }

        foreach(var dish in _createdDishes)
        {
            await dishApi.DeleteDish(dish.Id);
        }
    }

    public DishApiTests(ApiFixture fixture)
    {
        _productApi = new ProductApiClient(fixture.Client);
        _dishApi = new DishApiClient(fixture.Client);
    }


    [Fact(DisplayName = "Получение всех продуктов")]
    public async Task GetAllDishes_ReturnsOK()
    {
        var response = await _dishApi.GetAllDishes();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
}

