using System.Collections;

namespace Ecommerce.Domain;

public  class Tag : BaseEntity
{
    public int Id { get; set; }
    public required string Name { get; set; } = null!;
    
    public ICollection<Product> Products { get; set; } = [];
}
