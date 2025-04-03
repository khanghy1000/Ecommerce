namespace Ecommerce.Domain;

public class District
{
    public required int Id { get; set; }
    public required int ProvinceId { get; set; }
    public required string Name { get; set; }
    public ICollection<string> NameExtension { get; set; } = [];

    public Province Province { get; set; } = null!;
}
