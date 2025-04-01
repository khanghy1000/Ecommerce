using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Payments.DTOs;
using MediatR;

namespace Ecommerce.Application.Payments.Commands;

public class CreatePaymentUrl
{
    public class Command : IRequest<Result<string>>
    {
        public required CreatePaymentUrlDto CreatePaymentUrlDto { get; set; }
    }

    public class Handler(IPaymentService paymentService) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var result = await paymentService.CreatePaymentUrl(
                    request.CreatePaymentUrlDto.Money,
                    request.CreatePaymentUrlDto.Description
                );
                return Result<string>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message, 400);
            }
        }
    }
}
