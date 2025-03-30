using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class EditProduct
{
    public class Command : IRequest<Result<ProductDto>>
    {
        public required int Id { get; set; }
        public required EditProductDto ProductDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext
                .Products.Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            mapper.Map(request.ProductDto, product);

            product.Categories.Clear();
            if (request.ProductDto.CategoryIds.Count > 0)
            {
                var categories = await dbContext
                    .Categories.Where(c => request.ProductDto.CategoryIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);

                foreach (var category in categories)
                {
                    product.Categories.Add(category);
                }
            }

            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;
            return !result
                ? Result<ProductDto>.Failure("Failed to update the product", 400)
                : Result<ProductDto>.Success(mapper.Map<ProductDto>(product));
        }
    }
}
