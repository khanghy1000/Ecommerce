namespace Ecommerce.Application.Statistics.DTOs;

public class ShopPerformanceResponseDto
{
    public DateTime Time { get; set; }
    public int Quantity { get; set; }
    public decimal Value { get; set; }
    public int OrderCount { get; set; }
}
