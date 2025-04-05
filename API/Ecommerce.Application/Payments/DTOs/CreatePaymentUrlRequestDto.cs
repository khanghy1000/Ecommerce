namespace Ecommerce.Application.Payments.DTOs;

public class CreatePaymentUrlRequestDto
{
    public double Money { get; set; }
    public string Description { get; set; } = "";
}
