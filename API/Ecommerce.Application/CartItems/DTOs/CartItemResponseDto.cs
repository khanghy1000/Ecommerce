using Ecommerce.Application.Products.DTOs;

namespace Ecommerce.Application.CartItems.DTOs;

public class CartItemResponseDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string ProductImageUrl { get; set; } = "";
    public string ShopId { get; set; } = "";
    public string ShopName { get; set; } = "";
}
