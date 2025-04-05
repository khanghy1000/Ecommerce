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
    public class Command : IRequest<Result<SubcategoryResponseDto>>
    {
        public required CreateSubcategoryRequestDto CreateSubcategoryRequestDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<SubcategoryResponseDto>>
    {
        public async Task<Result<SubcategoryResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(
                c => c.Id == request.CreateSubcategoryRequestDto.CategoryId,
                cancellationToken
            );

            if (category == null)
                return Result<SubcategoryResponseDto>.Failure("Category not found", 400);

            var subcategory = new Subcategory
            {
                Name = request.CreateSubcategoryRequestDto.Name,
                CategoryId = category.Id,
            };

            dbContext.Subcategories.Add(subcategory);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<SubcategoryResponseDto>.Failure("Failed to create subcategory", 400);

            var newSubcategory = await dbContext
                .Subcategories.ProjectTo<SubcategoryResponseDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == subcategory.Id, cancellationToken);

            return Result<SubcategoryResponseDto>.Success(newSubcategory);
        }
    }
}
