using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VNPAY.NET.Models;

namespace Ecommerce.Application.Payments.Commands;

public class PaymentCallback
{
    public class Query : IRequest<Result<PaymentResult>> { }

    public class Handler(IPaymentService paymentService, AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<PaymentResult>>
    {
        public async Task<Result<PaymentResult>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            PaymentResult? result = null;
            try
            {
                result = await paymentService.GetPaymentResult();
            }
            catch (Exception ex)
            {
                return Result<PaymentResult>.Failure(ex.Message, 400);
            }

            if (result == null)
            {
                return Result<PaymentResult>.Failure("Failed to get payment result", 400);
            }

            result.Timestamp = DateTime.SpecifyKind(result.Timestamp, DateTimeKind.Utc);
            var resultPayment = mapper.Map<Payment>(result);
            dbContext.Payments.Add(resultPayment);
            await dbContext.SaveChangesAsync(cancellationToken);

            var orderIds = result.Description.Split(";").ToList();

            var salesOrders = await dbContext
                .SalesOrders.Where(so => orderIds.Contains(so.Id.ToString()))
                .ToListAsync(cancellationToken);

            if (salesOrders.Count == 0)
            {
                return Result<PaymentResult>.Failure("Sales orders not found", 400);
            }

            if (!resultPayment.IsSuccess)
            {
                foreach (var salesOrder in salesOrders)
                {
                    salesOrder.Payments.Add(resultPayment);
                    dbContext.SalesOrders.Update(salesOrder);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                return Result<PaymentResult>.Failure("Payment failed", 400);
            }

            foreach (var salesOrder in salesOrders)
            {
                salesOrder.Payments.Add(resultPayment);
                salesOrder.Status = SalesOrderStatus.PendingConfirmation;
                dbContext.SalesOrders.Update(salesOrder);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<PaymentResult>.Success(result);
        }
    }
}
