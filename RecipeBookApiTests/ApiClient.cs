using System.Net.Http.Json;
using RecipeBookWebApi.Dto;

namespace RecipeBookApiTests;

public class ApiClient
{
    private readonly HttpClient _client;

    public ApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> GetAllProducts()
    {
        return await _client.GetAsync("api/products");
    }
}
