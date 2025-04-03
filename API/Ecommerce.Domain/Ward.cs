namespace Ecommerce.Domain;

public class Ward
{
    public required int Id { get; set; }
    public required int DistrictId { get; set; }
    public required string Name { get; set; }
    public ICollection<string> NameExtension { get; set; } = [];

    public District District { get; set; } = null!;
}
