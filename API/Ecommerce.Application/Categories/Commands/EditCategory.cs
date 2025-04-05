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
    public class Command : IRequest<Result<CategoryWithoutChildResponseDto>>
    {
        public required int Id { get; set; }
        public required EditCategoryRequestDto EditCategoryRequestDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CategoryWithoutChildResponseDto>>
    {
        public async Task<Result<CategoryWithoutChildResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(
                c => c.Id == request.Id,
                cancellationToken
            );

            if (category == null)
                return Result<CategoryWithoutChildResponseDto>.Failure("Category not found", 400);

            category.Name = request.EditCategoryRequestDto.Name ?? category.Name;

            await dbContext.SaveChangesAsync(cancellationToken);

            var updatedCategory = await dbContext
                .Categories.ProjectTo<CategoryWithoutChildResponseDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == category.Id, cancellationToken);

            return Result<CategoryWithoutChildResponseDto>.Success(updatedCategory);
        }
    }
}
