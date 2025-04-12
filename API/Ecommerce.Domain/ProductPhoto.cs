using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Domain;

public class ProductPhoto
{
    [Key]
    public required string Key { get; set; }
    public required int ProductId { get; set; }
    public required int DisplayOrder { get; set; }
}
