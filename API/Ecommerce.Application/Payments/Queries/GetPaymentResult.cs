using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using MediatR;
using VNPAY.NET.Models;

namespace Ecommerce.Application.Payments.Queries;

public class GetPaymentResult
{
    public class Query : IRequest<Result<PaymentResult>> { }

    public class Handler(IPaymentService paymentService)
        : IRequestHandler<Query, Result<PaymentResult>>
    {
        public async Task<Result<PaymentResult>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var result = await paymentService.GetPaymentResult();
                return result == null
                    ? Result<PaymentResult>.Failure("Failed to get payment result", 400)
                    : Result<PaymentResult>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<PaymentResult>.Failure(ex.Message, 400);
            }
        }
    }
}
