
using RecipeBookWebApi.Dto;
using System.Collections.Generic;

namespace RecipeBookWebApi.ApiTests;

public class ApiFixture : IAsyncLifetime
{
    internal HttpClient Client { get; }
    
    private readonly List<ProductResponseDto> _createdProducts = new();
    private readonly List<DishResponseDto> _createdDishes = new();

    public ApiFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5265")
        };            
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        try
        {
            // Удаляем блюда первыми, чтобы избежать зависимостей
            foreach (var dish in _createdDishes)
            {
                var dishApi = new DishApiClient(Client);
                await dishApi.DeleteDish(dish.Id);
            }

            // Затем продукты
            foreach (var product in _createdProducts)
            {
                var productApi = new ProductApiClient(Client);
                await productApi.DeleteProductByIdAsync(product.Id);
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не прерываем очистку
            Console.WriteLine($"Error during cleanup: {ex.Message}");
        }
        finally
        {
            Client?.Dispose();
        }
    }

    // Методы для добавления созданных сущностей (вызываются из тестов)
    public void AddCreatedProduct(ProductResponseDto product) => _createdProducts.Add(product);
    public void AddCreatedDish(DishResponseDto dish) => _createdDishes.Add(dish);
}