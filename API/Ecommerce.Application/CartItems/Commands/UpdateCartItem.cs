using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.CartItems.Commands;

public static class UpdateCartItem
{
    public class Command : IRequest<Result<Unit>>
    {
        public required UpdateCartItemDto ItemDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetUserAsync();

            var cartItem = await dbContext.CartItems.FirstOrDefaultAsync(
                ci => ci.ProductId == request.ItemDto.ProductId && ci.UserId == user.Id,
                cancellationToken
            );

            if (cartItem == null)
                return Result<Unit>.Failure("Cart item not found", 404);

            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ItemDto.ProductId,
                cancellationToken
            );
            if (product == null)
                return Result<Unit>.Failure("Product not found", 404);

            if (product.Quantity < request.ItemDto.Quantity)
                return Result<Unit>.Failure("Not enough product in stock", 400);

            if (request.ItemDto.Quantity <= 0)
            {
                dbContext.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = request.ItemDto.Quantity;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
