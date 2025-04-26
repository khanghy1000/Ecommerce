using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Queries;

public static class ListSalesOrders
{
    public class Query : IRequest<Result<PagedList<SalesOrderResponseDto>>>
    {
        public int? OrderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public SalesOrderStatus? Status { get; set; }
        public string? BuyerId { get; set; }
        public string? ShopId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<PagedList<SalesOrderResponseDto>>>
    {
        public async Task<Result<PagedList<SalesOrderResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var user = await userAccessor.GetUserAsync();
            var userRole = userAccessor.GetUserRoles().First();

            var query = dbContext
                .SalesOrders.Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .AsQueryable();

            if (userRole == UserRole.Buyer.ToString())
            {
                query = query.Where(o => o.UserId == user.Id);
            }

            if (userRole == UserRole.Shop.ToString())
            {
                query = query.Where(o => o.OrderProducts.Any(op => op.Product.ShopId == user.Id));
            }

            if (request.OrderId.HasValue)
            {
                query = query.Where(o => o.Id == request.OrderId.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderTime >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.OrderTime <= request.ToDate.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(o => o.Status == request.Status.Value);
            }

            if (!string.IsNullOrEmpty(request.BuyerId))
            {
                query = query.Where(o => o.UserId == request.BuyerId);
            }

            if (!string.IsNullOrEmpty(request.ShopId))
            {
                query = query.Where(o =>
                    o.OrderProducts.Any(op => op.Product.ShopId == request.ShopId)
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.OrderTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<SalesOrderResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var pagedList = new PagedList<SalesOrderResponseDto>
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Items = orders,
            };

            return Result<PagedList<SalesOrderResponseDto>>.Success(pagedList);
        }
    }
}
