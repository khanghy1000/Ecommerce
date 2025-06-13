using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Reviews.Commands;

public static class CreateReview
{
    public class Command : IRequest<Result<ReviewResponseDto>>
    {
        public CreateReviewRequestDto ReviewDto { get; set; } = null!;
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<ReviewResponseDto>>
    {
        public async Task<Result<ReviewResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ReviewDto.ProductId,
                cancellationToken
            );

            if (product == null)
            {
                return Result<ReviewResponseDto>.Failure("Product not found", 404);
            }

            var userBoughtProduct = await dbContext.SalesOrders.AnyAsync(
                o =>
                    o.UserId == user.Id
                    && o.OrderProducts.Any(op => op.ProductId == request.ReviewDto.ProductId),
                cancellationToken
            );

            if (!userBoughtProduct)
            {
                return Result<ReviewResponseDto>.Failure(
                    "You can only review products you have purchased",
                    400
                );
            }

            var existingReview = await dbContext.ProductReviews.FirstOrDefaultAsync(
                r => r.ProductId == request.ReviewDto.ProductId && r.UserId == user.Id,
                cancellationToken
            );

            if (existingReview != null)
            {
                return Result<ReviewResponseDto>.Failure(
                    "You have already reviewed this product",
                    400
                );
            }

            var review = new ProductReview
            {
                ProductId = request.ReviewDto.ProductId,
                UserId = user.Id,
                Rating = request.ReviewDto.Rating,
                Review = request.ReviewDto.Review,
            };

            dbContext.ProductReviews.Add(review);

            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                return Result<ReviewResponseDto>.Failure("Failed to create review", 500);
            }

            await dbContext.Entry(review).Reference(r => r.User).LoadAsync(cancellationToken);
            return Result<ReviewResponseDto>.Success(mapper.Map<ReviewResponseDto>(review));
        }
    }
}
