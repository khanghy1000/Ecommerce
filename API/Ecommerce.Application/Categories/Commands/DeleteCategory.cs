using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Commands;

public static class DeleteCategory
{
    public class Command : IRequest<Result<Unit>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var category = await dbContext.Categories.FindAsync([request.Id], cancellationToken);

            if (category == null)
                return Result<Unit>.Failure("Category not found", 404);

            var hasChildren = await dbContext.Categories.AnyAsync(
                c => c.ParentId == request.Id,
                cancellationToken
            );

            if (hasChildren)
                return Result<Unit>.Failure(
                    "Cannot delete category with child categories. Remove child categories first.",
                    400
                );

            var hasProducts = await dbContext.ProductCategories.AnyAsync(
                p => p.CategoryId == request.Id,
                cancellationToken
            );

            if (hasProducts)
                return Result<Unit>.Failure(
                    "Cannot delete category that has products. Remove or reassign products first.",
                    400
                );

            dbContext.Categories.Remove(category);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return success
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("Failed to delete the category", 400);
        }
    }
}
