using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Test;

public class CartControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CartControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task EnsureSuccessWithContent(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception(
                $"Request failed with status code {response.StatusCode}: {content}"
            );
        }
    }

    [Fact]
    public async Task Add_Product_To_Cart()
    {
        var token = await new CustomWebApplicationFactory().GetAuthTokenAsync(
            _client,
            "a@b",
            "Test12345*"
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var response = await _client.PostAsJsonAsync(
            "/api/cart",
            new { ProductId = 1, Quantity = 1 }
        );
        await EnsureSuccessWithContent(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Increase_Cart_Product_Quantity()
    {
        var token = await new CustomWebApplicationFactory().GetAuthTokenAsync(
            _client,
            "a@b",
            "Test12345*"
        );
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );

        var response = await _client.PutAsJsonAsync(
            "/api/cart",
            new { ProductId = 1, Quantity = 2 }
        );
        await EnsureSuccessWithContent(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
