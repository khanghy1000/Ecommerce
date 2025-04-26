using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
                .SalesOrders.Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

            if (salesOrder == null)
            {
                return Result<SalesOrderResponseDto>.Failure("Sales order not found", 404);
            }

            var salesOrderDto = mapper.Map<SalesOrderResponseDto>(salesOrder);
            return Result<SalesOrderResponseDto>.Success(salesOrderDto);
        }
    }
}
