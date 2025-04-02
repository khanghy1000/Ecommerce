using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Shipments;

public class ShipmentService(IOptions<GHNSettings> config) : IShipmentService
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

    public async Task<GetShipmentDetailsResponse?> GetShipmentDetails(string shipmentOrderCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/shipping-order/detail");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(new { order_code = shipmentOrderCode });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GetShipmentDetailsResponse>(
            _jsonSerializerOptions
        );
    }

    public async Task<CreateShipmentResponse?> ReviewShipment(CreateShipmentRequest shipmentRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/shipping-order/preview");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(shipmentRequest, options: _jsonSerializerOptions);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateShipmentResponse>(
            _jsonSerializerOptions
        );
    }

    public async Task<CreateShipmentResponse?> CreateShipment(CreateShipmentRequest shipmentRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/shipping-order/create");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(shipmentRequest, options: _jsonSerializerOptions);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateShipmentResponse>(
            _jsonSerializerOptions
        );
    }

    public async Task<CancelShipmentResponse?> CancelShipment(string shipmentOrderCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/switch-status/cancel");
        request.Headers.Add("ShopId", _shopId);
        request.Headers.Add("Token", _token);
        request.Content = JsonContent.Create(new { order_codes = new[] { shipmentOrderCode } });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CancelShipmentResponse>(
            _jsonSerializerOptions
        );
    }
}
