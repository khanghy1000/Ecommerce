using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Commands;

public static class CreateSubcategory
{
    public class Command : IRequest<Result<SubcategoryDto>>
    {
        public required CreateSubcategoryDto SubcategoryDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<SubcategoryDto>>
    {
        public async Task<Result<SubcategoryDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(
                c => c.Id == request.SubcategoryDto.CategoryId,
                cancellationToken
            );

            if (category == null)
                return Result<SubcategoryDto>.Failure("Category not found", 400);

            var subcategory = new Subcategory
            {
                Name = request.SubcategoryDto.Name,
                CategoryId = category.Id,
            };

            dbContext.Subcategories.Add(subcategory);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<SubcategoryDto>.Failure("Failed to create subcategory", 400);

            var newSubcategory = await dbContext
                .Subcategories.ProjectTo<SubcategoryDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == subcategory.Id, cancellationToken);

            return Result<SubcategoryDto>.Success(newSubcategory);
        }
    }
}
