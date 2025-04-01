namespace Ecommerce.Application.Payments.DTOs;

public class CreatePaymentUrlDto
{
    public double Money { get; set; }
    public string Description { get; set; } = "";
}
