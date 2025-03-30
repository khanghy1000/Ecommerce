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
    public class Query : IRequest<Result<CategoryDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var category = await dbContext
                .Categories.ProjectTo<CategoryDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            return category == null
                ? Result<CategoryDto>.Failure("Category not found", 404)
                : Result<CategoryDto>.Success(category);
        }
    }
}
