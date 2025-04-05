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
    public class Command : IRequest<Result<CategoryWithoutChildResponseDto>>
    {
        public required CreateCategoryRequestDto CreateCategoryRequestDto { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<CategoryWithoutChildResponseDto>>
    {
        public async Task<Result<CategoryWithoutChildResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var category = new Category { Name = request.CreateCategoryRequestDto.Name };

            dbContext.Categories.Add(category);
            var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<CategoryWithoutChildResponseDto>.Failure(
                    "Failed to create the category",
                    400
                );

            var newCategory = await dbContext
                .Categories.ProjectTo<CategoryWithoutChildResponseDto>(mapper.ConfigurationProvider)
                .FirstAsync(c => c.Id == category.Id, cancellationToken);

            return Result<CategoryWithoutChildResponseDto>.Success(newCategory);
        }
    }
}
