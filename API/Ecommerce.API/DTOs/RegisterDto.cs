using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    public string Password { get; set; } = "";

    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role.")]
    public UserRole Role { get; set; } = UserRole.Buyer;
}
