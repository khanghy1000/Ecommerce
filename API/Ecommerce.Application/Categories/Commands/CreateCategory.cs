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

public static class CreateCategory
{
    public class Command : IRequest<Result<CategoryDto>>
    {
        public required CreateCategoryDto CategoryDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            if (request.CategoryDto.ParentId != null)
            {
                var parentExists = await dbContext.Categories.AnyAsync(
                    c => c.Id == request.CategoryDto.ParentId,
                    cancellationToken
                );

                if (!parentExists)
                    return Result<CategoryDto>.Failure("Parent category not found", 404);
            }

            var category = new Category
            {
                Name = request.CategoryDto.Name,
                ParentId = request.CategoryDto.ParentId,
            };

            dbContext.Categories.Add(category);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<CategoryDto>.Failure("Failed to create the category", 400);

            var newCategory = await dbContext
                .Categories.ProjectTo<CategoryDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == category.Id, cancellationToken);

            return Result<CategoryDto>.Success(newCategory);
        }
    }
}
