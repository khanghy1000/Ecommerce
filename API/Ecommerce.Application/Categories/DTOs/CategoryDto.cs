using System.Collections.Generic;

namespace Ecommerce.Application.Categories.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public string ParentName { get; set; }
    public List<CategoryDto> Children { get; set; }
}
