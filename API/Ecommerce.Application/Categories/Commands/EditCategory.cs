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
    public class Command : IRequest<Result<CategoryWithoutChildDto>>
    {
        public required int Id { get; set; }
        public required EditCategoryDto CategoryDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CategoryWithoutChildDto>>
    {
        public async Task<Result<CategoryWithoutChildDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(
                c => c.Id == request.Id,
                cancellationToken
            );

            if (category == null)
                return Result<CategoryWithoutChildDto>.Failure("Category not found", 400);

            category.Name = request.CategoryDto.Name ?? category.Name;

            await dbContext.SaveChangesAsync(cancellationToken);

            var updatedCategory = await dbContext
                .Categories.ProjectTo<CategoryWithoutChildDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == category.Id, cancellationToken);

            return Result<CategoryWithoutChildDto>.Success(updatedCategory);
        }
    }
}
