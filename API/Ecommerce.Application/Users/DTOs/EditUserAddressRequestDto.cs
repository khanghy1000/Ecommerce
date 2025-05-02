namespace Ecommerce.Application.Users.DTOs;

public class EditUserAddressRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int WardId { get; set; }
    public bool IsDefault { get; set; }
}
