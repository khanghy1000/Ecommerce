using System;

namespace Ecommerce.Application.Products.DTOs;

public class AddProductDiscountRequestDto
{
    public decimal DiscountPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
