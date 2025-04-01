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
    public class Query : IRequest<Result<List<CartItemDto>>> { }

    public class Handler(AppDbContext dbContext, IUserAccessor userAccessor, IMapper mapper)
        : IRequestHandler<Query, Result<List<CartItemDto>>>
    {
        public async Task<Result<List<CartItemDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var cartItems = await dbContext
                .CartItems.Where(ci => ci.UserId == user.Id)
                .ProjectTo<CartItemDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<CartItemDto>>.Success(cartItems);
        }
    }
}
