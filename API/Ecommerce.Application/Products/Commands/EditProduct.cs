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
    public class Command : IRequest<Result<ProductResponseDto>>
    {
        public required int Id { get; set; }
        public required EditProductRequestDto EditProductRequestDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<ProductResponseDto>>
    {
        public async Task<Result<ProductResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var product = await dbContext
                .Products.Include(p => p.Subcategories)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductResponseDto>.Failure("Product not found", 404);

            mapper.Map(request.EditProductRequestDto, product);

            product.Subcategories.Clear();
            if (request.EditProductRequestDto.SubcategoryIds.Count > 0)
            {
                var subcategories = await dbContext
                    .Subcategories.Where(sc =>
                        request.EditProductRequestDto.SubcategoryIds.Contains(sc.Id)
                    )
                    .ToListAsync(cancellationToken);

                foreach (var subcategory in subcategories)
                {
                    product.Subcategories.Add(subcategory);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<ProductResponseDto>.Success(mapper.Map<ProductResponseDto>(product));
        }
    }
}
