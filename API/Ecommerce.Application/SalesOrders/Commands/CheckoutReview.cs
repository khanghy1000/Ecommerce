using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.SalesOrders.Commands;

public class CheckoutReview
{
    public class Command : IRequest<Result<CheckoutPriceReviewResponseDto>>
    {
        public required CheckoutPriceReviewRequestDto CheckoutPriceReviewRequestDto { get; set; }
    }

    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor,
        IMapper mapper,
        IShippingService shippingService
    ) : IRequestHandler<Command, Result<CheckoutPriceReviewResponseDto>>
    {
        public async Task<Result<CheckoutPriceReviewResponseDto>> Handle(
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
                    x => x.Id == request.CheckoutPriceReviewRequestDto.ShippingWardId,
                    cancellationToken
                );

            if (shippingWard == null)
                return Result<CheckoutPriceReviewResponseDto>.Failure("Ward not found", 400);

            var cartItems = await dbContext
                .CartItems.Include(ci => ci.Product)
                .ThenInclude(p => p.Shop)
                .ThenInclude(u => u.Ward)
                .ThenInclude(w => w!.District)
                .ThenInclude(d => d.Province)
                .Where(ci => ci.UserId == user.Id)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                return Result<CheckoutPriceReviewResponseDto>.Failure("Cart is empty", 400);

            var groupedCartItems = cartItems.GroupBy(ci => ci.Product.Shop).ToList();
            var subtotal = (int)
                Math.Ceiling(
                    cartItems.Sum(ci =>
                        (ci.Product.DiscountPrice ?? ci.Product.RegularPrice) * ci.Quantity
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
                    ToName = request.CheckoutPriceReviewRequestDto.ShippingName,
                    ToPhone = request.CheckoutPriceReviewRequestDto.ShippingPhone,
                    ToAddress = request.CheckoutPriceReviewRequestDto.ShippingAddress,
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
                    var shippingResponse = await shippingService.ReviewShipping(shippingRequest);

                    var fee = shippingResponse?.Data?.TotalFee;
                    if (fee == null)
                        return Result<CheckoutPriceReviewResponseDto>.Failure(
                            "Shipping fee not found",
                            400
                        );
                    shippingFee += (int)fee;
                }
                catch (Exception ex)
                {
                    return Result<CheckoutPriceReviewResponseDto>.Failure(ex.Message, 500);
                }
            }

            return Result<CheckoutPriceReviewResponseDto>.Success(
                new CheckoutPriceReviewResponseDto
                {
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    Total = subtotal + shippingFee,
                }
            );
        }
    }
}
