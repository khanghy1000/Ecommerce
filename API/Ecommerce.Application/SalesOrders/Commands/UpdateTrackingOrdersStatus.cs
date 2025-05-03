using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class UpdateTrackingOrdersStatus
{
    public class Command : IRequest<Result<Unit>> { }

    public class Handler(AppDbContext dbContext, IShippingService shippingService)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var salesOrders = await dbContext
                .SalesOrders.Where(x => x.Status == SalesOrderStatus.Tracking)
                .ToListAsync(cancellationToken);

            foreach (var order in salesOrders)
            {
                if (order.ShippingOrderCode == null)
                {
                    continue;
                }

                var shippingInfo = await shippingService.GetShippingDetails(
                    order.ShippingOrderCode
                );

                if (shippingInfo?.Data == null)
                {
                    continue;
                }

                order.Status = shippingInfo.Data.Status switch
                {
                    "delivered" => SalesOrderStatus.Delivered,
                    "cancel" => SalesOrderStatus.Cancelled,
                    "returned" => SalesOrderStatus.Cancelled,
                    _ => order.Status,
                };
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
