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
    public class Command : IRequest<Result<ProductDto>>
    {
        public required CreateProductDto ProductDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();

            var product = mapper.Map<Product>(request.ProductDto);

            product.Shop = user;

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

            if (request.ProductDto.TagIds.Count > 0)
            {
                var tags = await dbContext
                    .Tags.Where(t => request.ProductDto.TagIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                foreach (var tag in tags)
                {
                    product.Tags.Add(tag);
                }
            }

            dbContext.Products.Add(product);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return !result
                ? Result<ProductDto>.Failure("Failed to create product", 400)
                : Result<ProductDto>.Success(mapper.Map<ProductDto>(product));
        }
    }
}
