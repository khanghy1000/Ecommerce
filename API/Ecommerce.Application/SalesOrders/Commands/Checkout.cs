using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class Checkout
{
    public class Command : IRequest<Result<CheckoutResponseDto>>
    {
        public required CheckoutRequestDto CheckoutRequestDto { get; set; }
    }

    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor,
        IMapper mapper,
        IShippingService shippingService,
        IPaymentService paymentService
    ) : IRequestHandler<Command, Result<CheckoutResponseDto>>
    {
        public async Task<Result<CheckoutResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            // TODO: Apply coupon

            var user = await userAccessor.GetUserAsync();

            var shippingWard = await dbContext
                .Wards.Include(w => w.District)
                .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(
                    x => x.Id == request.CheckoutRequestDto.ShippingWardId,
                    cancellationToken
                );

            if (shippingWard == null)
                return Result<CheckoutResponseDto>.Failure("Ward not found", 400);

            var cartItems = await dbContext
                .CartItems.Include(ci => ci.Product)
                .ThenInclude(p => p.Shop)
                .ThenInclude(u => u.Ward)
                .ThenInclude(w => w!.District)
                .ThenInclude(d => d.Province)
                .Where(ci => ci.UserId == user.Id)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                return Result<CheckoutResponseDto>.Failure("Cart is empty", 400);

            var groupedCartItems = cartItems.GroupBy(ci => ci.Product.Shop).ToList();

            var createdSalesOrders = new List<SalesOrder>();

            // Create order for each shop
            foreach (var cartItem in groupedCartItems)
            {
                var shop = cartItem.Key;
                var items = cartItem.ToList();
                var subTotal = (int)
                    Math.Ceiling(
                        items.Sum(i =>
                            i.Product.DiscountPrice ?? i.Product.RegularPrice * i.Quantity
                        )
                    );

                var shippingRequest = new CreateShippingRequest
                {
                    PaymentTypeId = 1,
                    RequiredNote = "CHOXEMHANGKHONGTHU",
                    FromName = shop.DisplayName!,
                    FromPhone = shop.PhoneNumber!,
                    FromAddress = shop.Address!,
                    FromWardName = shop.Ward!.Name,
                    FromDistrictName = shop.Ward.District.Name,
                    FromProvinceName = shop.Ward.District.Province.Name,
                    ToName = request.CheckoutRequestDto.ShippingName,
                    ToPhone = request.CheckoutRequestDto.ShippingPhone,
                    ToAddress = request.CheckoutRequestDto.ShippingAddress,
                    ToWardName = shippingWard.Name,
                    ToDistrictName = shippingWard.District.Name,
                    ToProvinceName = shippingWard.District.Province.Name,
                    CodAmount = 0,
                    Length = items.Max(i => i.Product.Length),
                    Width = items.Max(i => i.Product.Width),
                    Height = items.Max(i => i.Product.Height),
                    Weight = items.Max(i => i.Product.Weight),
                    Items = mapper.Map<List<CreateShippingRequestItem>>(items),
                };

                try
                {
                    var shippingResponse = await shippingService.PreviewShipping(shippingRequest);

                    var fee = shippingResponse?.Data?.TotalFee;
                    if (fee == null)
                        return Result<CheckoutResponseDto>.Failure("Shipping fee not found", 400);

                    var order = new SalesOrder
                    {
                        UserId = user.Id,
                        Subtotal = subTotal,
                        ShippingFee = (int)fee,
                        Total = subTotal + (int)fee,
                        ShippingName = request.CheckoutRequestDto.ShippingName,
                        ShippingPhone = request.CheckoutRequestDto.ShippingPhone,
                        ShippingAddress = request.CheckoutRequestDto.ShippingAddress,
                        ShippingWardId = request.CheckoutRequestDto.ShippingWardId,
                        PaymentMethod = request.CheckoutRequestDto.PaymentMethod,
                        Status =
                            request.CheckoutRequestDto.PaymentMethod == PaymentMethod.Cod
                                ? SalesOrderStatus.PendingConfirmation
                                : SalesOrderStatus.PendingPayment,
                    };

                    dbContext.SalesOrders.Add(order);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    createdSalesOrders.Add(order);

                    var orderProducts = new List<OrderProduct>();
                    foreach (var item in items)
                    {
                        var orderProduct = new OrderProduct
                        {
                            Name = item.Product.Name,
                            Price = item.Product.DiscountPrice ?? item.Product.RegularPrice,
                            Quantity = item.Quantity,
                            Subtotal =
                                (item.Product.DiscountPrice ?? item.Product.RegularPrice)
                                * item.Quantity,
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                        };
                        orderProducts.Add(orderProduct);
                    }

                    dbContext.OrderProducts.AddRange(orderProducts);
                    dbContext.CartItems.RemoveRange(cartItems);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    return Result<CheckoutResponseDto>.Failure(ex.Message, 500);
                }
            }

            if (request.CheckoutRequestDto.PaymentMethod != PaymentMethod.Vnpay)
                return Result<CheckoutResponseDto>.Success(
                    new CheckoutResponseDto
                    {
                        SalesOrders = mapper.Map<List<SalesOrderResponseDto>>(createdSalesOrders),
                    }
                );
            try
            {
                var orderIds = string.Join(";", createdSalesOrders.Select(so => so.Id));
                var url = await paymentService.CreatePaymentUrl(
                    (int)createdSalesOrders.Sum(so => so.Total)!,
                    orderIds
                );
                if (string.IsNullOrEmpty(url))
                    return Result<CheckoutResponseDto>.Failure("Failed to create payment url", 400);
                return Result<CheckoutResponseDto>.Success(
                    new CheckoutResponseDto
                    {
                        PaymentUrl = url,
                        SalesOrders = mapper.Map<List<SalesOrderResponseDto>>(createdSalesOrders),
                    }
                );
            }
            catch (Exception ex)
            {
                return Result<CheckoutResponseDto>.Failure(ex.Message, 500);
            }
        }
    }
}
