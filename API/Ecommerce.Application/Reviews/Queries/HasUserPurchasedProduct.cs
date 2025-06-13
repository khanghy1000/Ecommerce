using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Reviews.Queries;

public static class HasUserPurchasedProduct
{
    public class Query : IRequest<Result<HasUserPurchasedProductDto>>
    {
        public int ProductId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<HasUserPurchasedProductDto>>
    {
        public async Task<Result<HasUserPurchasedProductDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var product = await dbContext.Products.FirstOrDefaultAsync(
                p => p.Id == request.ProductId,
                cancellationToken
            );

            if (product == null)
            {
                return Result<HasUserPurchasedProductDto>.Failure("Product not found", 404);
            }

            var userBoughtProduct = await dbContext.SalesOrders.AnyAsync(
                o =>
                    o.UserId == user.Id
                    && o.OrderProducts.Any(op => op.ProductId == request.ProductId),
                cancellationToken
            );

            return Result<HasUserPurchasedProductDto>.Success(
                new HasUserPurchasedProductDto
                {
                    HasPurchased = userBoughtProduct,
                    UserId = user.Id,
                    ProductId = request.ProductId,
                }
            );
        }
    }
}
