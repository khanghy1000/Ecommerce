namespace Ecommerce.Application.Users.DTOs;

public class UserAddressResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int WardId { get; set; }
    public int DistrictId { get; set; }
    public int ProvinceId { get; set; }
    public string WardName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
