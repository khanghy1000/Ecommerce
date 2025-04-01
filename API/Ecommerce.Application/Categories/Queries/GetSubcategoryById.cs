using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Core;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.Queries;

public static class GetSubcategoryById
{
    public class Query : IRequest<Result<SubcategoryDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<SubcategoryDto>>
    {
        public async Task<Result<SubcategoryDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var subcategory = await dbContext
                .Subcategories.ProjectTo<SubcategoryDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(sc => sc.Id == request.Id, cancellationToken);

            return subcategory == null
                ? Result<SubcategoryDto>.Failure("Subcategory not found", 404)
                : Result<SubcategoryDto>.Success(subcategory);
        }
    }
}
