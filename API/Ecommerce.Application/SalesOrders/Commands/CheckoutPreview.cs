using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class CheckoutPreview
{
    public class Command : IRequest<Result<CheckoutPricePreviewResponseDto>>
    {
        public required CheckoutPricePreviewRequestDto CheckoutPricePreviewRequestDto { get; set; }
    }

    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor,
        IMapper mapper,
        IShippingService shippingService
    ) : IRequestHandler<Command, Result<CheckoutPricePreviewResponseDto>>
    {
        public async Task<Result<CheckoutPricePreviewResponseDto>> Handle(
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
                    x => x.Id == request.CheckoutPricePreviewRequestDto.ShippingWardId,
                    cancellationToken
                );

            if (shippingWard == null)
                return Result<CheckoutPricePreviewResponseDto>.Failure("Ward not found", 400);

            var cartItems = await dbContext
                .CartItems.Include(ci => ci.Product)
                .ThenInclude(p => p.Shop)
                .ThenInclude(u => u.Ward)
                .ThenInclude(w => w!.District)
                .ThenInclude(d => d.Province)
                .Include(cartItem => cartItem.Product)
                .ThenInclude(product => product.Discounts)
                .Where(ci => ci.UserId == user.Id)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                return Result<CheckoutPricePreviewResponseDto>.Failure("Cart is empty", 400);

            var groupedCartItems = cartItems.GroupBy(ci => ci.Product.Shop).ToList();
            var subtotal = (int)
                Math.Ceiling(
                    cartItems.Sum(ci =>
                        (
                            ci.Product.Discounts.Where(d =>
                                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                                )
                                .OrderBy(d => d.DiscountPrice)
                                .Select(d => (decimal?)d.DiscountPrice)
                                .FirstOrDefault() ?? ci.Product.RegularPrice
                        ) * ci.Quantity
                    )
                );
            int shippingFee = 0;

            foreach (var cartItem in groupedCartItems)
            {
                var shop = cartItem.Key;
                var items = cartItem.ToList();

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
                    ToName = request.CheckoutPricePreviewRequestDto.ShippingName,
                    ToPhone = request.CheckoutPricePreviewRequestDto.ShippingPhone,
                    ToAddress = request.CheckoutPricePreviewRequestDto.ShippingAddress,
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
                        return Result<CheckoutPricePreviewResponseDto>.Failure(
                            "Shipping fee not found",
                            400
                        );
                    shippingFee += (int)fee;
                }
                catch (Exception ex)
                {
                    return Result<CheckoutPricePreviewResponseDto>.Failure(ex.Message, 500);
                }
            }

            return Result<CheckoutPricePreviewResponseDto>.Success(
                new CheckoutPricePreviewResponseDto
                {
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    Total = subtotal + shippingFee,
                }
            );
        }
    }
}
