using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class ConfirmOrder
{
    public class Command : IRequest<Result<SalesOrderResponseDto>>
    {
        public required int Id { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IMediator mediator)
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

            if (salesOrder.Status != SalesOrderStatus.PendingConfirmation)
                return Result<SalesOrderResponseDto>.Failure(
                    "Sales order is not in pending confirmation status",
                    400
                );

            var createShippingOrderResult = await mediator.Send(
                new CreateShippingOrder.Command { SalesOrderId = salesOrder.Id },
                cancellationToken
            );

            if (!createShippingOrderResult.IsSuccess)
                return Result<SalesOrderResponseDto>.Failure(
                    createShippingOrderResult.Error!,
                    createShippingOrderResult.Code
                );

            salesOrder.Status = SalesOrderStatus.Tracking;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<SalesOrderResponseDto>.Success(
                mapper.Map<SalesOrderResponseDto>(salesOrder)
            );
        }
    }
}
