using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Queries;

public static class GetSalesOrderById
{
    public class Query : IRequest<Result<SalesOrderResponseDto>>
    {
        public int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<SalesOrderResponseDto>>
    {
        public async Task<Result<SalesOrderResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var salesOrder = await dbContext
                .SalesOrders.Where(o => o.Id == request.Id)
                .ProjectTo<SalesOrderResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (salesOrder == null)
            {
                return Result<SalesOrderResponseDto>.Failure("Sales order not found", 404);
            }

            return Result<SalesOrderResponseDto>.Success(salesOrder);
        }
    }
}
