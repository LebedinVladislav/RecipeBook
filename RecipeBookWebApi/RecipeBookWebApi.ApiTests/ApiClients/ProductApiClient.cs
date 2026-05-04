using System.Net.Http.Json;
using RecipeBookWebApi.Dto;

namespace RecipeBookWebApi.ApiTests
{
    public class ProductApiClient
    {
        private readonly HttpClient _client;

        public ProductApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetAllProducts()
        {
            return await _client.GetAsync("api/products");
        }

        public async Task<HttpResponseMessage> GetProduct(int id)
        {
            return await _client.GetAsync($"api/products/{id}");
        }

        public async Task<HttpResponseMessage> GetProducts(string query = "")
        {
            string url = string.IsNullOrWhiteSpace(query) ? "api/products" : $"api/products?{query}";
            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> CreateProduct(ProductRequestDto data)
        {
            return await _client.PostAsJsonAsync("api/products", data);
        }

        public async Task<HttpResponseMessage> DeleteProduct(int id)
        {
            return await _client.DeleteAsync($"api/products/{id}");
        }
    }
}
