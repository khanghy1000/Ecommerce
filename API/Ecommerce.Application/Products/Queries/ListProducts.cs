using System.ComponentModel.DataAnnotations;
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
    public class Query : IRequest<Result<PagedList<ProductDto>>>
    {
        public string? Keyword { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public List<int>? CategoryIds { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<ProductDto>>>
    {
        public async Task<Result<PagedList<ProductDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = dbContext
                .Products.Include(x => x.Categories)
                .Include(x => x.Shop)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x =>
                    x.SearchVector.Matches(
                        EF.Functions.PlainToTsQuery("vietnamese", request.Keyword)
                    )
                );
            }

            if (request.CategoryIds is { Count: > 0 })
            {
                query = query.Where(x => x.Categories.Any(c => request.CategoryIds.Contains(c.Id)));
            }

            query = request.SortBy switch
            {
                "price" => request.SortDirection == "asc"
                    ? query.OrderBy(x => x.DiscountPrice ?? x.RegularPrice)
                    : query.OrderByDescending(x => x.DiscountPrice ?? x.RegularPrice),
                "name" => request.SortDirection == "asc"
                    ? query.OrderBy(x => x.Name)
                    : query.OrderByDescending(x => x.Name),
                _ => query,
            };

            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return Result<PagedList<ProductDto>>.Success(
                new PagedList<ProductDto>
                {
                    Items = mapper.Map<List<ProductDto>>(products),
                    TotalCount = await query.CountAsync(cancellationToken),
                    PageSize = request.PageSize,
                    PageNumber = request.PageNumber,
                }
            );
        }
    }
}
