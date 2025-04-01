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
    public class Command : IRequest<Result<CategoryWithoutChildDto>>
    {
        public required CreateCategoryDto CategoryDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CategoryWithoutChildDto>>
    {
        public async Task<Result<CategoryWithoutChildDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = new Category { Name = request.CategoryDto.Name };

            dbContext.Categories.Add(category);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<CategoryWithoutChildDto>.Failure(
                    "Failed to create the category",
                    400
                );

            var newCategory = await dbContext
                .Categories.ProjectTo<CategoryWithoutChildDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == category.Id, cancellationToken);

            return Result<CategoryWithoutChildDto>.Success(newCategory);
        }
    }
}
