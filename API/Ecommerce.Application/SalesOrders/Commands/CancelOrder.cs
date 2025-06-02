using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class CancelOrder
{
    public class Command : IRequest<Result<SalesOrderResponseDto>>
    {
        public required int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Command, Result<SalesOrderResponseDto>>
    {
        public async Task<Result<SalesOrderResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var salesOrder = await dbContext.SalesOrders.FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken
            );

            if (salesOrder == null)
                return Result<SalesOrderResponseDto>.Failure("Sales order not found", 400);

            if (
                salesOrder.Status != SalesOrderStatus.PendingConfirmation
                && salesOrder.Status != SalesOrderStatus.PendingPayment
            )
                return Result<SalesOrderResponseDto>.Failure(
                    "Sales order is not in pending confirmation or pending payment status",
                    400
                );

            salesOrder.Status = SalesOrderStatus.Cancelled;
            await dbContext.SaveChangesAsync(cancellationToken);

            var resultOrder = await dbContext
                .SalesOrders.ProjectTo<SalesOrderResponseDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (resultOrder == null)
                return Result<SalesOrderResponseDto>.Failure("Sales order not found", 404);

            return Result<SalesOrderResponseDto>.Success(resultOrder);
        }
    }
}
