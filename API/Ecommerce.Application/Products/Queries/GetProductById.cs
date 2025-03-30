using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class GetProductById
{
    public class Query : IRequest<Result<ProductDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext
                .Products.ProjectTo<ProductDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            return Result<ProductDto>.Success(product);
        }
    }
}
