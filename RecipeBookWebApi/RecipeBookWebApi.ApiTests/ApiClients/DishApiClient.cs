using System.Net.Http.Json;
using RecipeBookWebApi.Dto;

namespace RecipeBookWebApi.ApiTests;

public class DishApiClient
{
    private readonly HttpClient _client;

    public DishApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> GetAllDishes(string query = "")
    {
        string url = string.IsNullOrWhiteSpace(query) ? "api/dishes" : $"api/dishes?{query}";
        return await _client.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetDish(int id)
    {
        return await _client.GetAsync($"api/dishes/{id}");
    }

    public async Task<HttpResponseMessage> CreateDish(DishRequestDto data)
    {
        return await _client.PostAsJsonAsync("api/dishes", data);
    }

    public async Task<HttpResponseMessage> UpdateDish(int id, DishRequestDto data)
    {
        return await _client.PutAsJsonAsync($"api/dishes/{id}", data);
    }

    public async Task<HttpResponseMessage> DeleteDish(int id)
    {
        return await _client.DeleteAsync($"api/dishes/{id}");
    }

    public async Task<HttpResponseMessage> CalculateNutrition(NutritionCalculateRequestDto data)
    {
        return await _client.PostAsJsonAsync("api/dishes/calculate-nutrition", data);
    }
}
