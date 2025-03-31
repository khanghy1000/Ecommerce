using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.DTOs;

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = "";

    [Required]
    public string NewPassword { get; set; } = "";
}
