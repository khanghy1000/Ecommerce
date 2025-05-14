using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Reviews.Queries;

public static class ListProductReviews
{
    public class Query : IRequest<Result<PagedList<ReviewResponseDto>>>
    {
        public int? ProductId { get; set; }
        public string? UserId { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<PagedList<ReviewResponseDto>>>
    {
        public async Task<Result<PagedList<ReviewResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = dbContext.ProductReviews.AsQueryable();

            if (request.ProductId.HasValue)
            {
                query = query.Where(x => x.ProductId == request.ProductId.Value);
            }

            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(x => x.UserId == request.UserId);
            }

            var reviews = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<ReviewResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<PagedList<ReviewResponseDto>>.Success(
                new PagedList<ReviewResponseDto>
                {
                    Items = reviews,
                    TotalCount = await query.CountAsync(cancellationToken),
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                }
            );
        }
    }
}
