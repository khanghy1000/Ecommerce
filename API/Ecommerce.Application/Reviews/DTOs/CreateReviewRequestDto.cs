namespace Ecommerce.Application.Reviews.DTOs;

public class CreateReviewRequestDto
{
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
}
