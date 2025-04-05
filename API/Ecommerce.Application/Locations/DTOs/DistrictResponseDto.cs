namespace Ecommerce.Application.Locations.DTOs;

public class DistrictResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; } = "";
}
