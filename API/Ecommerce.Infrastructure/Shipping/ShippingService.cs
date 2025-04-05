using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Shipping;

public class ShippingService(IOptions<GHNSettings> config) : IShippingService
{
    private readonly string _shopId = config.Value.ShopId;
    private readonly string _clientId = config.Value.ClientId;
    private readonly string _token = config.Value.Token;
    private readonly string _baseUrl = config.Value.BaseUrl;
    private readonly HttpClient _httpClient = new();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
    };

    public async Task<GetShippingDetailsResponse?> GetShippingDetails(string shippingOrderCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/shipping-order/detail");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(new { order_code = shippingOrderCode });

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<GetShippingDetailsResponse>(
            _jsonSerializerOptions
        );
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(content?.Message ?? "Failed to get shipping details");
        }
        return content;
    }

    public async Task<CreateShippingResponse?> PreviewShipping(
        CreateShippingRequest shippingRequest
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/shipping-order/preview");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(shippingRequest, options: _jsonSerializerOptions);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<CreateShippingResponse>(
            _jsonSerializerOptions
        );
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(content?.Message ?? "Failed to get shipping preview");
        }
        return content;
    }

    public async Task<CreateShippingResponse?> CreateShipping(CreateShippingRequest shippingRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/shipping-order/create");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(shippingRequest, options: _jsonSerializerOptions);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<CreateShippingResponse>(
            _jsonSerializerOptions
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(content?.Message ?? "Failed to create shipping");
        }
        return content;
    }

    public async Task<CancelShippingResponse?> CancelShipping(string shippingOrderCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/switch-status/cancel");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(new { order_codes = new[] { shippingOrderCode } });

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<CancelShippingResponse>(
            _jsonSerializerOptions
        );
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(content?.Message ?? "Failed to cancel shipping");
        }
        return content;
    }
}
