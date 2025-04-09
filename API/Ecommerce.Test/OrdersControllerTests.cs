using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Test;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Checkout_Cart()
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

        var requestPayload = new
        {
            ShippingName = "John Doe",
            ShippingPhone = "0346014147",
            ShippingAddress = "123 Main St, City, Country",
            ShippingWardId = 1,
            PaymentMethod = "Cod",
        };

        var response = await _client.PostAsJsonAsync("/api/orders/checkout", requestPayload);
        var content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
