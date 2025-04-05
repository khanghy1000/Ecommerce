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
    public class Query : IRequest<Result<SubcategoryResponseDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<SubcategoryResponseDto>>
    {
        public async Task<Result<SubcategoryResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var subcategory = await dbContext
                .Subcategories.ProjectTo<SubcategoryResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(sc => sc.Id == request.Id, cancellationToken);

            return subcategory == null
                ? Result<SubcategoryResponseDto>.Failure("Subcategory not found", 404)
                : Result<SubcategoryResponseDto>.Success(subcategory);
        }
    }
}
