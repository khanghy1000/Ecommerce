namespace Ecommerce.Domain;

public class Coupon : BaseEntity
{
    public int Id { get; set; }
    public required string Code { get; set; } = null!;
    public required string Description { get; set; }
    public bool? Active { get; set; }
    public required decimal Value { get; set; }
    public required bool Multiple { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    public ICollection<SalesOrder> SalesOrders { get; set; } = [];
}
