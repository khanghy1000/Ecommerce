using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Test;

public class IdentityControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IdentityControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Success()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/login",
            new { Email = "a@b", Password = "Test12345*" }
        );
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_Failed()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/login",
            new { Email = "wrong@b", Password = "WrongPassword" }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
