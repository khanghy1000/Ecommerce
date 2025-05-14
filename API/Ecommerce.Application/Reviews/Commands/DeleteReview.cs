using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Reviews.Commands;

public static class DeleteReview
{
    public class Command : IRequest<Result<Unit>>
    {
        public int ReviewId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetUserAsync();
            var userRoles = userAccessor.GetUserRoles();

            var review = await dbContext.ProductReviews.FirstOrDefaultAsync(
                r => r.Id == request.ReviewId,
                cancellationToken
            );

            if (review == null)
            {
                return Result<Unit>.Failure("Review not found", 404);
            }

            dbContext.ProductReviews.Remove(review);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !success
                ? Result<Unit>.Failure("Failed to delete review", 500)
                : Result<Unit>.Success(Unit.Value);
        }
    }
}
