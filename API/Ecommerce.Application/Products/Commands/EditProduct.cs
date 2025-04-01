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
                .Products.Include(p => p.Subcategories)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found", 404);

            mapper.Map(request.ProductDto, product);

            product.Subcategories.Clear();
            if (request.ProductDto.SubcategoryIds.Count > 0)
            {
                var subcategories = await dbContext
                    .Subcategories.Where(sc => request.ProductDto.SubcategoryIds.Contains(sc.Id))
                    .ToListAsync(cancellationToken);

                foreach (var subcategory in subcategories)
                {
                    product.Subcategories.Add(subcategory);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<ProductDto>.Success(mapper.Map<ProductDto>(product));
        }
    }
}
