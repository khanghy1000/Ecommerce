namespace Ecommerce.Application.Users.DTOs;

public class AddAddressRequestDto
{
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Address { get; set; } = "";
    public int WardId { get; set; }
    public bool IsDefault { get; set; }
}
