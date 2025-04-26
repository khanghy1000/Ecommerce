using System.Net.Http.Json;
using Ecommerce.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ecommerce.Persistence;

public static class Seed
{
    public static async Task SeedLocations(AppDbContext context, string token)
    {
        if (context.Provinces.Any() || context.Districts.Any() || context.Wards.Any())
        {
            return;
        }

        var httpClient = new HttpClient();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/province"
        );
        request.Headers.Add("Token", token);
        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadFromJsonAsync<ProvinceResponse>();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Failed to get provinces");
        }

        if (content == null)
        {
            throw new HttpRequestException("Failed to parse provinces");
        }
        foreach (var province in content.Data)
        {
            var newProvince = new Province
            {
                Id = (int)province.ProvinceId,
                Name = province.ProvinceName,
                NameExtension = province.NameExtension?.ToList() ?? new List<string>(),
            };
            await context.Provinces.AddAsync(newProvince);
        }

        foreach (var province in content.Data)
        {
            var requestDistrict = new HttpRequestMessage(
                HttpMethod.Post,
                "https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district"
            );
            requestDistrict.Headers.Add("Token", token);
            requestDistrict.Content = JsonContent.Create(new { province_id = province.ProvinceId });
            var responseDistrict = await httpClient.SendAsync(requestDistrict);
            var contentDistrict =
                await responseDistrict.Content.ReadFromJsonAsync<DistrictResponse>();

            if (!responseDistrict.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Failed to get districts");
            }

            if (contentDistrict == null)
            {
                throw new HttpRequestException("Failed to parse districts");
            }

            if (contentDistrict?.Data == null)
            {
                continue;
            }
            foreach (var district in contentDistrict.Data)
            {
                var newDistrict = new District
                {
                    Id = (int)district.DistrictId,
                    ProvinceId = (int)district.ProvinceId,
                    Name = district.DistrictName,
                    NameExtension = district.NameExtension?.ToList() ?? new List<string>(),
                };
                await context.Districts.AddAsync(newDistrict);
            }

            foreach (var district in contentDistrict.Data)
            {
                var requestWard = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={district.DistrictId}"
                );
                requestWard.Headers.Add("Token", token);
                var responseWard = await httpClient.SendAsync(requestWard);
                var contentWard = await responseWard.Content.ReadFromJsonAsync<WardResponse>();

                if (!responseWard.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Failed to get wards");
                }

                if (contentWard == null)
                {
                    throw new HttpRequestException("Failed to parse wards");
                }

                if (contentWard?.Data == null)
                {
                    continue;
                }

                foreach (var ward in contentWard.Data)
                {
                    if (!int.TryParse(ward.WardCode, out int wardId))
                    {
                        Console.WriteLine($"Skipping ward with invalid code: {ward.WardCode}");
                        continue;
                    }
                    var newWard = new Ward
                    {
                        Id = wardId,
                        DistrictId = (int)ward.DistrictId,
                        Name = ward.WardName,
                        NameExtension = ward.NameExtension?.ToList() ?? new List<string>(),
                    };
                    await context.Wards.AddAsync(newWard);
                }
            }
        }
        await context.SaveChangesAsync();
    }
}

public class ProvinceResponse
{
    public long Code { get; set; }
    public string Message { get; set; }
    public ProvinceData[] Data { get; set; }
}

public class ProvinceData
{
    public long ProvinceId { get; set; }
    public string ProvinceName { get; set; }
    public string[] NameExtension { get; set; }
}

public class DistrictResponse
{
    public long Code { get; set; }
    public string Message { get; set; }
    public DistrictData[] Data { get; set; }
}

public class DistrictData
{
    public long DistrictId { get; set; }
    public long ProvinceId { get; set; }
    public string DistrictName { get; set; }
    public string[] NameExtension { get; set; }
}

public class WardResponse
{
    public long Code { get; set; }
    public string Message { get; set; }
    public WardData[] Data { get; set; }
}

public partial class WardData
{
    public string WardCode { get; set; }
    public long DistrictId { get; set; }
    public string WardName { get; set; }
    public string[] NameExtension { get; set; }
}
