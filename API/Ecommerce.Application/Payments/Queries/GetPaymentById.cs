using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Payments.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Payments.Queries;

public class GetPaymentById
{
    public class Query : IRequest<Result<PaymentResponseDto>>
    {
        public long PaymentId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<PaymentResponseDto>>
    {
        public async Task<Result<PaymentResponseDto>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var payment = await dbContext.Payments.FirstOrDefaultAsync(
                p => p.PaymentId == request.PaymentId,
                cancellationToken
            );

            return payment == null
                ? Result<PaymentResponseDto>.Failure("Payment not found", 404)
                : Result<PaymentResponseDto>.Success(mapper.Map<PaymentResponseDto>(payment));
        }
    }
}
