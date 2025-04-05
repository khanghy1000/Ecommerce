namespace Ecommerce.Application.Locations.DTOs;

public class WardResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = "";
}
