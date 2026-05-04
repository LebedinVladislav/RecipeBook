
using RecipeBookWebApi.ApiTests.Tests;

namespace RecipeBookWebApi.ApiTests;


public class ApiFixture : IDisposable
{
    internal HttpClient Client { get; }
    
    public ApiFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5265")
        };            
    }

    public void Dispose()
    {
        ProductApiTests.DeleteCreatedEntities(Client).GetAwaiter().GetResult();
        Client.Dispose();
    }
}