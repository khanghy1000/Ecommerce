using System;

namespace Ecommerce.Application.Products.DTOs;

public class EditProductDiscountRequestDto
{
    public decimal DiscountPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
