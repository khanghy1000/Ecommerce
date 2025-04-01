using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Commands;

public static class EditCategory
{
    public class Command : IRequest<Result<CategoryIdNameDto>>
    {
        public required int Id { get; set; }
        public required EditCategoryDto CategoryDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CategoryIdNameDto>>
    {
        public async Task<Result<CategoryIdNameDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(
                c => c.Id == request.Id,
                cancellationToken
            );

            if (category == null)
                return Result<CategoryIdNameDto>.Failure("Category not found", 400);

            // Check for circular reference if updating parent ID
            if (
                request.CategoryDto.ParentId != null
                && request.CategoryDto.ParentId != category.ParentId
            )
            {
                // Ensure parent category exists
                var parentExists = await dbContext.Categories.AnyAsync(
                    c => c.Id == request.CategoryDto.ParentId,
                    cancellationToken
                );

                if (!parentExists)
                    return Result<CategoryIdNameDto>.Failure("Parent category not found", 400);

                // Prevent setting a category as its own parent
                if (request.CategoryDto.ParentId == category.Id)
                    return Result<CategoryIdNameDto>.Failure(
                        "A category cannot be its own parent",
                        400
                    );

                // Prevent circular references in category hierarchy
                var potentialParentId = request.CategoryDto.ParentId;
                while (potentialParentId != null)
                {
                    var parent = await dbContext.Categories.FirstOrDefaultAsync(
                        c => c.Id == potentialParentId,
                        cancellationToken
                    );

                    if (parent == null)
                        break;

                    if (parent.ParentId == category.Id)
                        return Result<CategoryIdNameDto>.Failure(
                            "Circular reference detected in category hierarchy",
                            400
                        );

                    potentialParentId = parent.ParentId;
                }
            }

            category.Name = request.CategoryDto.Name ?? category.Name;
            category.ParentId = request.CategoryDto.ParentId;

            await dbContext.SaveChangesAsync(cancellationToken);

            var updatedCategory = await dbContext
                .Categories.ProjectTo<CategoryIdNameDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == category.Id, cancellationToken);

            return Result<CategoryIdNameDto>.Success(updatedCategory);
        }
    }
}
