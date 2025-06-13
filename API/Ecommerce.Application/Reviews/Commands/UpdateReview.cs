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

public static class UpdateReview
{
    public class Command : IRequest<Result<ReviewResponseDto>>
    {
        public int ReviewId { get; set; }
        public UpdateReviewRequestDto ReviewDto { get; set; } = null!;
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<ReviewResponseDto>>
    {
        public async Task<Result<ReviewResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var review = await dbContext
                .ProductReviews.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
            {
                return Result<ReviewResponseDto>.Failure("Review not found", 400);
            }

            review.Rating = request.ReviewDto.Rating;
            review.Review = request.ReviewDto.Review;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<ReviewResponseDto>.Success(mapper.Map<ReviewResponseDto>(review));
        }
    }
}
