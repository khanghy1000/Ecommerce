using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public class GetPopularProducts
{
    public class Query : IRequest<Result<List<PopularProductResponseDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<PopularProductResponseDto>>>
    {
        public async Task<Result<List<PopularProductResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var popularProducts = await dbContext
                .PopularProducts.ProjectTo<PopularProductResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<PopularProductResponseDto>>.Success(popularProducts);
        }
    }
}
