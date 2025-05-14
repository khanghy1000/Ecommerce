using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Reviews.Queries;

public static class GetReviewById
{
    public class Query : IRequest<Result<ReviewResponseDto>>
    {
        public int ReviewId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<ReviewResponseDto>>
    {
        public async Task<Result<ReviewResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var review = await dbContext
                .ProductReviews.ProjectTo<ReviewResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            return review == null
                ? Result<ReviewResponseDto>.Failure("Review not found", 404)
                : Result<ReviewResponseDto>.Success(review);
        }
    }
}
