using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.CartItems.Commands;

public static class RemoveFromCart
{
    public class Command : IRequest<Result<Unit>>
    {
        public required RemoveFromCartDto ItemDto { get; set; }
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

            dbContext.CartItems.Remove(cartItem);

            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return result
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("Failed to remove item from cart", 400);
        }
    }
}
