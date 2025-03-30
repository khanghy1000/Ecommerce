using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Domain;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<Product> CreatedProducts { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
}
