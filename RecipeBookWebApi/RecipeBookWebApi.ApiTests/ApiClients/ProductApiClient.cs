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

        public async Task<HttpResponseMessage> GetProductByIdAsync(int id)
        {
            return await _client.GetAsync($"api/products/{id}");
        }

        public async Task<HttpResponseMessage> GetAllProductsAsync(string query = "")
        {
            string url = string.IsNullOrWhiteSpace(query) ? "api/products" : $"api/products?{query}";
            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> CreateProductAsync(ProductRequestDto data)
        {
            return await _client.PostAsJsonAsync("api/products", data);
        }

        public async Task<HttpResponseMessage> DeleteProductByIdAsync(int id)
        {
            return await _client.DeleteAsync($"api/products/{id}");
        }
    }
}
