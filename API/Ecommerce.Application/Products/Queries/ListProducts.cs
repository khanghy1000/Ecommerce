using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class ListProducts
{
    public class Query : IRequest<Result<List<ProductDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var products = await dbContext
                .Products.ProjectTo<ProductDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            return Result<List<ProductDto>>.Success(products);
        }
    }
}
