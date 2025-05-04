using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.Queries;

public static class ListProducts
{
    public class Query : IRequest<Result<PagedList<ProductResponseDto>>>
    {
        public string? Keyword { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int? CategoryId { get; set; }
        public List<int>? SubcategoryIds { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<ProductResponseDto>>>
    {
        public async Task<Result<PagedList<ProductResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            if (request is { CategoryId: not null, SubcategoryIds.Count: > 0 })
            {
                return Result<PagedList<ProductResponseDto>>.Failure(
                    "Cannot filter by both category and subcategory.",
                    400
                );
            }

            var query = dbContext.Products.AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x =>
                    x.SearchVector.Matches(
                        EF.Functions.PlainToTsQuery("vietnamese", request.Keyword)
                    )
                );
            }

            if (request.SubcategoryIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.Subcategories.Any(sc => request.SubcategoryIds.Contains(sc.Id))
                );
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(x => x.RegularPrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(x => x.RegularPrice <= request.MaxPrice.Value);
            }

            query = request.SortBy switch
            {
                "price" => request.SortDirection == "asc"
                    ? query.OrderBy(x =>
                        x.Discounts.Where(d =>
                                d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                            )
                            .OrderBy(d => d.DiscountPrice)
                            .Select(d => (decimal?)d.DiscountPrice)
                            .FirstOrDefault() ?? x.RegularPrice
                    )
                    : query.OrderByDescending(x =>
                        x.Discounts.Where(d =>
                                d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                            )
                            .OrderBy(d => d.DiscountPrice)
                            .Select(d => (decimal?)d.DiscountPrice)
                            .FirstOrDefault() ?? x.RegularPrice
                    ),
                "name" => request.SortDirection == "asc"
                    ? query.OrderBy(x => x.Name)
                    : query.OrderByDescending(x => x.Name),
                _ => query,
            };

            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<ProductResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                product.Photos = product.Photos.OrderBy(x => x.DisplayOrder).ToList();
            }

            return Result<PagedList<ProductResponseDto>>.Success(
                new PagedList<ProductResponseDto>
                {
                    Items = products,
                    TotalCount = await query.CountAsync(cancellationToken),
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber,
                }
            );
        }
    }
}
