using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Queries;

public static class GetCategoryById
{
    public class Query : IRequest<Result<CategoryResponseDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<CategoryResponseDto>>
    {
        public async Task<Result<CategoryResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext
                .Categories.ProjectTo<CategoryResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            return category == null
                ? Result<CategoryResponseDto>.Failure("Category not found", 404)
                : Result<CategoryResponseDto>.Success(category);
        }
    }
}
