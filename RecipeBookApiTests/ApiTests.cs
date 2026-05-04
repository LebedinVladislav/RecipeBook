using System.Net;
using RecipeBookWebApi.Dto;


namespace RecipeBookApiTests;


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
        Client.Dispose();
    }
}


public class ApiTests : IClassFixture<ApiFixture>
{
    private readonly ApiClient _api;

    public ApiTests(ApiFixture fixture)
    {
        _api = new ApiClient(fixture.Client);
    }

    [Fact]
    public async Task GetAllProducts_ReturnOK()
    {
        var response = await _api.GetAllProducts();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

