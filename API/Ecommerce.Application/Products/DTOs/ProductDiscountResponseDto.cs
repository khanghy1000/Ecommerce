using System;

namespace Ecommerce.Application.Products.DTOs;

public class ProductDiscountResponseDto
{
    public int Id { get; set; }
    public decimal DiscountPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal RegularPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
