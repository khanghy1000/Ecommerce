using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.CartItems.Queries;

public static class GetCartItems
{
    public class Query : IRequest<Result<List<CartItemResponseDto>>> { }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor, IMapper mapper)
        : IRequestHandler<Query, Result<List<CartItemResponseDto>>>
    {
        public async Task<Result<List<CartItemResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var inactiveCartItems = await dbContext
                .CartItems.Where(ci => ci.UserId == user.Id && !ci.Product.Active)
                .ToListAsync(cancellationToken);

            if (inactiveCartItems.Count != 0)
            {
                dbContext.CartItems.RemoveRange(inactiveCartItems);
            }

            var excessiveCartItems = await dbContext
                .CartItems.Where(ci => ci.Quantity > ci.Product.Quantity)
                .Include(cartItem => cartItem.Product)
                .ToListAsync(cancellationToken);

            if (excessiveCartItems.Count != 0)
            {
                foreach (var cartItem in excessiveCartItems)
                {
                    cartItem.Quantity = cartItem.Product.Quantity;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var cartItems = await dbContext
                .CartItems.Where(ci => ci.UserId == user.Id)
                .ProjectTo<CartItemResponseDto>(mapper.ConfigurationProvider)
                .OrderBy(ci => ci.ProductName)
                .ToListAsync(cancellationToken);

            return Result<List<CartItemResponseDto>>.Success(cartItems);
        }
    }
}
