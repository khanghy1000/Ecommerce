using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Commands;

public static class DeleteSubcategory
{
    public class Command : IRequest<Result<Unit>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var subcategory = await dbContext
                .Subcategories.Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (subcategory == null)
                return Result<Unit>.Failure("Subcategory not found", 404);

            if (subcategory.Products.Any())
                return Result<Unit>.Failure(
                    "Cannot delete subcategory with associated products",
                    400
                );

            dbContext.Subcategories.Remove(subcategory);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<Unit>.Failure("Failed to delete subcategory", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
