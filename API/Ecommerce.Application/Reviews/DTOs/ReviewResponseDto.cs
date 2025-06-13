namespace Ecommerce.Application.Reviews.DTOs;

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string UserId { get; set; } = null!;
    public string UserDisplayName { get; set; } = null!;
    public string? UserImageUrl { get; set; } = null!;
    public int Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
