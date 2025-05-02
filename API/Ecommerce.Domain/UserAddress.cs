namespace Ecommerce.Domain;

public class UserAddress
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required int WardId { get; set; }
    public required string UserId { get; set; }
    public required bool IsDefault { get; set; }

    public Ward Ward { get; set; } = null!;
    public User User { get; set; } = null!;
}
