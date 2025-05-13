using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class DeleteProductDiscount
{
    public class Command : IRequest<Result<Unit>>
    {
        public int ProductId { get; set; }
        public int DiscountId { get; set; }
    }

    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ProductId,
                cancellationToken
            );

            if (product == null)
                return Result<Unit>.Failure("Product not found", 400);

            var discount = await dbContext.ProductDiscounts.FirstOrDefaultAsync(
                d => d.Id == request.DiscountId && d.ProductId == request.ProductId,
                cancellationToken
            );

            if (discount == null)
                return Result<Unit>.Failure("Discount not found", 400);

            dbContext.ProductDiscounts.Remove(discount);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !success
                ? Result<Unit>.Failure("Failed to delete product discount", 400)
                : Result<Unit>.Success(Unit.Value);
        }
    }
}
