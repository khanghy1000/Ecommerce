using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Domain;

public class User: IdentityUser
{
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public string? ImageUrl { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = [];
}