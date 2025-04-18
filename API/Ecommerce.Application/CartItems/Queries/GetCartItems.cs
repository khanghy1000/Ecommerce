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

            var cartItems = await dbContext
                .CartItems.Where(ci => ci.UserId == user.Id)
                .ProjectTo<CartItemResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<CartItemResponseDto>>.Success(cartItems);
        }
    }
}
