using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Queries;

public static class ListCategories
{
    public class Query : IRequest<Result<List<CategoryDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<CategoryDto>>>
    {
        public async Task<Result<List<CategoryDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var categories = await dbContext
                .Categories.Where(c => c.ParentId == null)
                .ProjectTo<CategoryDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<CategoryDto>>.Success(categories);
        }
    }
}
