namespace Ecommerce.Domain;

public class Province
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<string> NameExtension { get; set; } = [];
}
