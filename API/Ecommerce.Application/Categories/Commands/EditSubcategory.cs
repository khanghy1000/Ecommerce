using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Commands;

public static class EditSubcategory
{
    public class Command : IRequest<Result<SubcategoryDto>>
    {
        public required int Id { get; set; }
        public required EditSubcategoryDto SubcategoryDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<SubcategoryDto>>
    {
        public async Task<Result<SubcategoryDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var subcategory = await dbContext.Subcategories.FirstOrDefaultAsync(
                s => s.Id == request.Id,
                cancellationToken
            );

            if (subcategory == null)
                return Result<SubcategoryDto>.Failure("Subcategory not found", 404);

            if (subcategory.CategoryId != request.SubcategoryDto.CategoryId)
            {
                var categoryExists = await dbContext.Categories.AnyAsync(
                    c => c.Id == request.SubcategoryDto.CategoryId,
                    cancellationToken
                );

                if (!categoryExists)
                    return Result<SubcategoryDto>.Failure("Target category not found", 400);
            }

            subcategory.Name = request.SubcategoryDto.Name;
            subcategory.CategoryId = request.SubcategoryDto.CategoryId;

            await dbContext.SaveChangesAsync(cancellationToken);

            var updatedSubcategory = await dbContext
                .Subcategories.ProjectTo<SubcategoryDto>(mapper.ConfigurationProvider)
                .FirstAsync(s => s.Id == subcategory.Id, cancellationToken);

            return Result<SubcategoryDto>.Success(updatedSubcategory);
        }
    }
}
