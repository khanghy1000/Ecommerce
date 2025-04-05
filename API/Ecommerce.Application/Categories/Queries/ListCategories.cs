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
    public class Query : IRequest<Result<List<CategoryResponseDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<CategoryResponseDto>>>
    {
        public async Task<Result<List<CategoryResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var categories = await dbContext
                .Categories.ProjectTo<CategoryResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<CategoryResponseDto>>.Success(categories);
        }
    }
}
