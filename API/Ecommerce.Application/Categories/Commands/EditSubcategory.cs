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
    public class Command : IRequest<Result<SubcategoryResponseDto>>
    {
        public required int Id { get; set; }
        public required EditSubcategoryRequestDto EditSubcategoryRequestDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<SubcategoryResponseDto>>
    {
        public async Task<Result<SubcategoryResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var subcategory = await dbContext.Subcategories.FirstOrDefaultAsync(
                s => s.Id == request.Id,
                cancellationToken
            );

            if (subcategory == null)
                return Result<SubcategoryResponseDto>.Failure("Subcategory not found", 404);

            if (subcategory.CategoryId != request.EditSubcategoryRequestDto.CategoryId)
            {
                var categoryExists = await dbContext.Categories.AnyAsync(
                    c => c.Id == request.EditSubcategoryRequestDto.CategoryId,
                    cancellationToken
                );

                if (!categoryExists)
                    return Result<SubcategoryResponseDto>.Failure("Target category not found", 400);
            }

            subcategory.Name = request.EditSubcategoryRequestDto.Name;
            subcategory.CategoryId = request.EditSubcategoryRequestDto.CategoryId;

            await dbContext.SaveChangesAsync(cancellationToken);

            var updatedSubcategory = await dbContext
                .Subcategories.ProjectTo<SubcategoryResponseDto>(mapper.ConfigurationProvider)
                .FirstAsync(s => s.Id == subcategory.Id, cancellationToken);

            return Result<SubcategoryResponseDto>.Success(updatedSubcategory);
        }
    }
}
