using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class CreateShippingOrder
{
    public class Command : IRequest<Result<Unit>>
    {
        public required int SalesOrderId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper, IShippingService shippingService)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var salesOrder = await dbContext
                .SalesOrders.Include(so => so.OrderProducts)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Shop)
                .ThenInclude(u => u.Ward)
                .ThenInclude(w => w!.District)
                .ThenInclude(d => d.Province)
                .Include(so => so.ShippingWard)
                .ThenInclude(w => w.District)
                .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(x => x.Id == request.SalesOrderId, cancellationToken);

            if (salesOrder == null)
                return Result<Unit>.Failure("Sales order not found", 400);

            var shop = salesOrder.OrderProducts.First().Product.Shop;

            var shippingRequest = new CreateShippingRequest
            {
                PaymentTypeId = salesOrder.PaymentMethod == PaymentMethod.Cod ? 2 : 1,
                RequiredNote = "CHOXEMHANGKHONGTHU",
                FromName = shop.DisplayName!,
                FromPhone = shop.PhoneNumber!,
                FromAddress = shop.Address!,
                FromWardName = shop.Ward!.Name,
                FromDistrictName = shop.Ward.District.Name,
                FromProvinceName = shop.Ward.District.Province.Name,
                ToName = salesOrder.ShippingName,
                ToPhone = salesOrder.ShippingPhone,
                ToAddress = salesOrder.ShippingAddress,
                ToWardName = salesOrder.ShippingWard.Name,
                ToDistrictName = salesOrder.ShippingWard.District.Name,
                ToProvinceName = salesOrder.ShippingWard.District.Province.Name,
                CodAmount =
                    salesOrder.PaymentMethod == PaymentMethod.Cod
                        ? (int)Math.Ceiling(salesOrder.Subtotal)
                        : 0,
                Length = salesOrder.OrderProducts.Max(p => p.Product.Length),
                Width = salesOrder.OrderProducts.Max(p => p.Product.Width),
                Height = salesOrder.OrderProducts.Max(p => p.Product.Height),
                Weight = salesOrder.OrderProducts.Sum(p => p.Product.Weight),
                Items = mapper.Map<List<CreateShippingRequestItem>>(salesOrder.OrderProducts),
            };
            try
            {
                var shippingResponse = await shippingService.CreateShipping(shippingRequest);

                var shippingOrderCode = shippingResponse?.Data?.OrderCode;
                var shippingFee = shippingResponse?.Data?.TotalFee;

                if (string.IsNullOrEmpty(shippingOrderCode))
                    return Result<Unit>.Failure("Failed to create shipping", 400);

                if (!shippingFee.HasValue)
                    return Result<Unit>.Failure("Shipping fee not found", 400);

                // TODO: Update order status
            }
            catch (Exception ex)
            {
                return Result<Unit>.Failure(ex.Message, 500);
            }
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
