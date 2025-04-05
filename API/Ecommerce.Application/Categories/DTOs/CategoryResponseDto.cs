using System.Collections.Generic;
using Ecommerce.Domain;

namespace Ecommerce.Application.Categories.DTOs;

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<SubcategoryIdNameResponseDto> Subcategories { get; set; } = null!;
}
