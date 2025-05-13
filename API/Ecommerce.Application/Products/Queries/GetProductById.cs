using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class GetProductById
{
    public class Query : IRequest<Result<ProductResponseDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<ProductResponseDto>>
    {
        public async Task<Result<ProductResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext
                .Products.ProjectTo<ProductResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductResponseDto>.Failure("Product not found", 404);

            if (!product.Active)
            {
                User user;
                string userRole;

                try
                {
                    user = await userAccessor.GetUserAsync();
                    userRole = userAccessor.GetUserRoles().First();
                }
                catch (Exception ex)
                {
                    return Result<ProductResponseDto>.Failure("Product not found", 404);
                }

                if (
                    userRole == nameof(UserRole.Buyer)
                    || userRole == nameof(UserRole.Shop) && product.ShopId != user.Id
                )
                    return Result<ProductResponseDto>.Failure("Product not found", 404);
            }

            return Result<ProductResponseDto>.Success(product);
        }
    }
}
