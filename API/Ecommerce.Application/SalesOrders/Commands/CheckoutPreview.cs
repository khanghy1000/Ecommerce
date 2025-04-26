using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Coupons.Commands;
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
        IShippingService shippingService,
        IMediator mediator
    ) : IRequestHandler<Command, Result<CheckoutPricePreviewResponseDto>>
    {
        public async Task<Result<CheckoutPricePreviewResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
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

            var cartItems = await GetCartItemsAsync(user.Id, cancellationToken);

            if (cartItems.Count == 0)
                return Result<CheckoutPricePreviewResponseDto>.Failure("Cart is empty", 400);

            var groupedCartItems = cartItems.GroupBy(ci => ci.Product.Shop).ToList();

            // Calculate original subtotal (before coupon discount)
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

            var getShippingFeeResult = await GetShippingFeeAsync(
                request,
                groupedCartItems,
                shippingWard
            );
            if (!getShippingFeeResult.IsSuccess)
            {
                return Result<CheckoutPricePreviewResponseDto>.Failure(
                    getShippingFeeResult.Error!,
                    getShippingFeeResult.Code
                );
            }
            var shippingFee = getShippingFeeResult.Value;

            var productCategoryIds = cartItems
                .SelectMany(ci => ci.Product.Subcategories.Select(s => s.CategoryId))
                .Distinct()
                .ToList();

            Coupon? productCoupon = null;
            Coupon? shippingCoupon = null;

            if (!string.IsNullOrEmpty(request.CheckoutPricePreviewRequestDto.ProductCouponCode))
            {
                var productCouponResult = await mediator.Send(
                    new ValidateCoupon.Command
                    {
                        CouponCode = request.CheckoutPricePreviewRequestDto.ProductCouponCode,
                        CouponType = CouponType.Product,
                        OrderSubtotal = subtotal,
                        ProductCategoryIds = productCategoryIds,
                    },
                    cancellationToken
                );

                if (!productCouponResult.IsSuccess)
                    return Result<CheckoutPricePreviewResponseDto>.Failure(
                        productCouponResult.Error!,
                        productCouponResult.Code
                    );
                productCoupon = productCouponResult.Value;
            }

            if (!string.IsNullOrEmpty(request.CheckoutPricePreviewRequestDto.ShippingCouponCode))
            {
                var shippingCouponResult = await mediator.Send(
                    new ValidateCoupon.Command
                    {
                        CouponCode = request.CheckoutPricePreviewRequestDto.ShippingCouponCode,
                        CouponType = CouponType.Shipping,
                        OrderSubtotal = subtotal,
                        ProductCategoryIds = productCategoryIds,
                    },
                    cancellationToken
                );

                if (!shippingCouponResult.IsSuccess)
                    return Result<CheckoutPricePreviewResponseDto>.Failure(
                        shippingCouponResult.Error!,
                        shippingCouponResult.Code
                    );
                shippingCoupon = shippingCouponResult.Value;
            }

            var productDiscount = GetProductDiscount(productCoupon, cartItems, subtotal);
            var shippingDiscount = GetShippingDiscount(shippingCoupon, shippingFee);

            var discountedSubtotal = subtotal - (int)Math.Ceiling(productDiscount);
            var discountedShippingFee = (int)Math.Ceiling(shippingFee - shippingDiscount);
            var total = discountedSubtotal + discountedShippingFee;

            return Result<CheckoutPricePreviewResponseDto>.Success(
                new CheckoutPricePreviewResponseDto
                {
                    Subtotal = subtotal,
                    ShippingFee = shippingFee,
                    ProductDiscountAmount = (decimal)Math.Ceiling(productDiscount),
                    ShippingDiscountAmount = (decimal)Math.Ceiling(shippingDiscount),
                    Total = total,
                    AppliedProductCoupon = productCoupon?.Code,
                    AppliedShippingCoupon = shippingCoupon?.Code,
                }
            );
        }

        private async Task<List<CartItem>> GetCartItemsAsync(
            string userId,
            CancellationToken cancellationToken
        )
        {
            var cartItems = await dbContext
                .CartItems.Include(ci => ci.Product)
                .ThenInclude(p => p.Shop)
                .ThenInclude(u => u.Ward)
                .ThenInclude(w => w!.District)
                .ThenInclude(d => d.Province)
                .Include(cartItem => cartItem.Product)
                .ThenInclude(product => product.Discounts)
                .Include(cartItem => cartItem.Product.Subcategories)
                .ThenInclude(s => s.Category)
                .Where(ci => ci.UserId == userId)
                .ToListAsync(cancellationToken);
            return cartItems;
        }

        private decimal GetShippingDiscount(Coupon? shippingCoupon, int shippingFee)
        {
            if (shippingCoupon == null)
                return 0;

            decimal shippingDiscount = 0;
            if (shippingCoupon is { DiscountType: CouponDiscountType.Percent })
            {
                shippingDiscount = shippingFee * (shippingCoupon.Value / 100);
            }
            if (shippingCoupon is { DiscountType: CouponDiscountType.Amount })
            {
                shippingDiscount = shippingCoupon.Value;
            }

            if (shippingDiscount > shippingCoupon.MaxDiscountAmount)
            {
                shippingDiscount = shippingCoupon.MaxDiscountAmount;
            }

            if (shippingDiscount > shippingFee)
            {
                shippingDiscount = shippingFee;
            }

            return shippingDiscount;
        }

        private decimal GetProductDiscount(
            Coupon? productCoupon,
            List<CartItem> cartItems,
            int subtotal
        )
        {
            decimal productDiscount = 0;

            if (productCoupon == null)
                return 0;

            if (productCoupon is { Categories.Count: > 0 })
            {
                // Apply discount only to products with matching categories
                decimal categorySpecificTotal = 0;
                foreach (var item in cartItems)
                {
                    if (
                        !item.Product.Subcategories.Any(subcategory =>
                            productCoupon.Categories.Any(cc => cc.Id == subcategory.CategoryId)
                        )
                    )
                        continue;

                    var itemPrice =
                        item.Product.Discounts.Where(d =>
                                d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                            )
                            .OrderBy(d => d.DiscountPrice)
                            .Select(d => (decimal?)d.DiscountPrice)
                            .FirstOrDefault() ?? item.Product.RegularPrice;

                    categorySpecificTotal += itemPrice * item.Quantity;
                }

                if (productCoupon.DiscountType == CouponDiscountType.Percent)
                {
                    productDiscount = categorySpecificTotal * (productCoupon.Value / 100);
                }
                else
                {
                    productDiscount =
                        productCoupon.Value <= categorySpecificTotal
                            ? productCoupon.Value
                            : categorySpecificTotal;
                }
            }
            else if (productCoupon is { Categories.Count: 0 })
            {
                if (productCoupon.DiscountType == CouponDiscountType.Percent)
                {
                    productDiscount = subtotal * (productCoupon.Value / 100);
                }
                else
                {
                    productDiscount =
                        productCoupon.Value <= subtotal ? productCoupon.Value : subtotal;
                }
            }

            if (productDiscount > productCoupon.MaxDiscountAmount)
            {
                productDiscount = productCoupon.MaxDiscountAmount;
            }

            return productDiscount;
        }

        private async Task<Result<int>> GetShippingFeeAsync(
            Command request,
            List<IGrouping<User, CartItem>> groupedCartItems,
            Ward shippingWard
        )
        {
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
                        return Result<int>.Failure("Shipping fee not found", 400);
                    shippingFee += (int)fee;
                }
                catch (Exception ex)
                {
                    return Result<int>.Failure(ex.Message, 500);
                }
            }

            return Result<int>.Success(shippingFee);
        }
    }
}
