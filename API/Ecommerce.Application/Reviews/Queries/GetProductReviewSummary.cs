using Ecommerce.Application.Core;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Reviews.Queries;

public static class GetProductReviewSummary
{
    public class Query : IRequest<Result<ProductReviewSummaryDto>>
    {
        public int ProductId { get; set; }
    }

    public class Handler(AppDbContext dbContext)
        : IRequestHandler<Query, Result<ProductReviewSummaryDto>>
    {
        public async Task<Result<ProductReviewSummaryDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var productExists = await dbContext.Products.AnyAsync(
                p => p.Id == request.ProductId,
                cancellationToken
            );

            if (!productExists)
            {
                return Result<ProductReviewSummaryDto>.Failure("Product not found", 404);
            }

            // Get all reviews for the product
            var reviews = await dbContext
                .ProductReviews.Where(r => r.ProductId == request.ProductId)
                .Select(r => r.Rating)
                .ToListAsync(cancellationToken);

            var totalReviews = reviews.Count;
            var averageRating = totalReviews > 0 ? reviews.Average() : 0.0;

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviews.Count(r => r == i);
            }

            var summary = new ProductReviewSummaryDto
            {
                ProductId = request.ProductId,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingDistribution = ratingDistribution,
            };

            return Result<ProductReviewSummaryDto>.Success(summary);
        }
    }
}
