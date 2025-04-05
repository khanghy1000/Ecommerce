using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain;

namespace Ecommerce.API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    public string Password { get; set; } = "";

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = "";

    [Required]
    [StringLength(255)]
    public string Address { get; set; } = "";

    [Required]
    public int WardId { get; set; }

    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role.")]
    public UserRole Role { get; set; } = UserRole.Buyer;
}
