using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Commands;

public static class CreateProduct
{
    public class Command : IRequest<Result<ProductResponseDto>>
    {
        public required CreateProductRequestDto CreateProductRequestDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<ProductResponseDto>>
    {
        public async Task<Result<ProductResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var product = mapper.Map<Product>(request.CreateProductRequestDto);

            product.Shop = user;

            if (request.CreateProductRequestDto.SubcategoryIds.Count > 0)
            {
                var subcategories = await dbContext
                    .Subcategories.Where(sc =>
                        request.CreateProductRequestDto.SubcategoryIds.Contains(sc.Id)
                    )
                    .ToListAsync(cancellationToken);

                foreach (var subcategory in subcategories)
                {
                    product.Subcategories.Add(subcategory);
                }
            }

            dbContext.Products.Add(product);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !result
                ? Result<ProductResponseDto>.Failure("Failed to create product", 400)
                : Result<ProductResponseDto>.Success(mapper.Map<ProductResponseDto>(product));
        }
    }
}
