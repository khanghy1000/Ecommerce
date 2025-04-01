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

            var hasSubcategories = await dbContext.Subcategories.AnyAsync(
                sc => sc.CategoryId == request.Id,
                cancellationToken
            );

            if (hasSubcategories)
                return Result<Unit>.Failure(
                    "Cannot delete category with subcategories. Remove subcategories first.",
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
