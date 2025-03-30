using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class DeleteProduct
{
    public class Command : IRequest<Result<Unit>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.Id,
                cancellationToken
            );

            if (product == null)
                return Result<Unit>.Failure("Product not found", 404);

            dbContext.Products.Remove(product);

            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !result
                ? Result<Unit>.Failure("Failed to delete the product", 400)
                : Result<Unit>.Success(Unit.Value);
        }
    }
}
