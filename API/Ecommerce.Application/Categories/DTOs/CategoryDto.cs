using System.Collections.Generic;
using Ecommerce.Domain;

namespace Ecommerce.Application.Categories.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<SubcategoryNameDto> Subcategories { get; set; } = null!;
}
