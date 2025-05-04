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

            var cartItems = await GetCartItemsAsync(
                user.Id,
                request.CheckoutPricePreviewRequestDto.ProductIds,
                cancellationToken
            );

            if (cartItems.Count == 0)
                return Result<CheckoutPricePreviewResponseDto>.Failure("Cart is empty", 400);

            var groupedCartItems = cartItems
                .GroupBy(ci => ci.Product.Shop)
                .ToDictionary(g => g.Key, g => g.ToList());

            Dictionary<User, int> shopSubtotals = new();

            foreach (var group in groupedCartItems)
            {
                var shopSubtotal = (int)
                    Math.Ceiling(group.Value.Sum(ci => GetProductPrice(ci.Product) * ci.Quantity));
                shopSubtotals.Add(group.Key, shopSubtotal);
            }

            Dictionary<User, UserAddress> shopAddresses = new();

            foreach (var shop in groupedCartItems.Keys)
            {
                var shopAddress = await dbContext
                    .UserAddresses.Include(ua => ua.Ward)
                    .ThenInclude(w => w.District)
                    .ThenInclude(d => d.Province)
                    .FirstOrDefaultAsync(
                        ua => ua.UserId == shop.Id && ua.IsDefault == true,
                        cancellationToken
                    );

                if (shopAddress == null)
                    return Result<CheckoutPricePreviewResponseDto>.Failure(
                        "Shop address not found",
                        400
                    );

                shopAddresses.Add(shop, shopAddress);
            }

            Dictionary<User, int> shopShippingFees = new();

            foreach (var shop in groupedCartItems.Keys)
            {
                var getShippingFeeResult = await GetShippingFeeAsync(
                    request,
                    shopAddresses[shop],
                    groupedCartItems[shop],
                    shippingWard
                );
                if (!getShippingFeeResult.IsSuccess)
                {
                    return Result<CheckoutPricePreviewResponseDto>.Failure(
                        getShippingFeeResult.Error!,
                        getShippingFeeResult.Code
                    );
                }

                shopShippingFees.Add(shop, getShippingFeeResult.Value);
            }

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
                        OrderSubtotal = shopSubtotals.Values.Sum(),
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
                        OrderSubtotal = shopSubtotals.Values.Sum(),
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

            var productDiscounts = CalculateProductDiscounts(
                productCoupon,
                groupedCartItems,
                shopSubtotals
            );
            var shippingDiscounts = CalculateShippingDiscount(
                shippingCoupon,
                groupedCartItems,
                shopShippingFees
            );

            var subtotal = shopSubtotals.Values.Sum();
            var totalShippingFee = shopShippingFees.Values.Sum();
            var productDiscount = Math.Ceiling(productDiscounts.Values.Sum());
            var shippingDiscount = Math.Ceiling(shippingDiscounts.Values.Sum());
            var total = subtotal + totalShippingFee - productDiscount - shippingDiscount;

            return Result<CheckoutPricePreviewResponseDto>.Success(
                new CheckoutPricePreviewResponseDto
                {
                    Subtotal = subtotal,
                    ShippingFee = totalShippingFee,
                    ProductDiscountAmount = productDiscount,
                    ShippingDiscountAmount = shippingDiscount,
                    Total = total,
                    AppliedProductCoupon = productCoupon?.Code,
                    AppliedShippingCoupon = shippingCoupon?.Code,
                }
            );
        }

        private async Task<List<CartItem>> GetCartItemsAsync(
            string userId,
            List<int> productIds,
            CancellationToken cancellationToken
        )
        {
            var cartItems = await dbContext
                .CartItems.Include(ci => ci.Product)
                .ThenInclude(p => p.Shop)
                .Include(cartItem => cartItem.Product)
                .ThenInclude(product => product.Discounts)
                .Include(cartItem => cartItem.Product.Subcategories)
                .ThenInclude(s => s.Category)
                .Where(ci => ci.UserId == userId && productIds.Contains(ci.ProductId))
                .ToListAsync(cancellationToken);
            return cartItems;
        }

        private Dictionary<User, decimal> CalculateShippingDiscount(
            Coupon? shippingCoupon,
            Dictionary<User, List<CartItem>> groupedCartItems,
            Dictionary<User, int> shopShippingFees
        )
        {
            if (shippingCoupon == null)
                return new Dictionary<User, decimal>();

            Dictionary<User, decimal> shippingDiscounts = new();

            if (shippingCoupon is { Categories.Count: > 0 })
            {
                var eligibleGroups = groupedCartItems
                    .Where(group =>
                        group.Value.Any(item =>
                            item.Product.Subcategories.Any(subcategory =>
                                shippingCoupon.Categories.Any(c => c.Id == subcategory.CategoryId)
                            )
                        )
                    )
                    .ToList();

                if (eligibleGroups.Count == 0)
                    return shippingDiscounts;

                // Only for shops with eligible products
                decimal categorySpecificShippingFee = eligibleGroups.Sum(group =>
                    shopShippingFees.GetValueOrDefault(group.Key)
                );

                if (categorySpecificShippingFee == 0)
                    return shippingDiscounts;

                decimal totalShippingDiscount;

                if (shippingCoupon.DiscountType == CouponDiscountType.Percent)
                {
                    totalShippingDiscount =
                        categorySpecificShippingFee * (shippingCoupon.Value / 100);
                }
                else
                {
                    totalShippingDiscount =
                        shippingCoupon.Value <= categorySpecificShippingFee
                            ? shippingCoupon.Value
                            : categorySpecificShippingFee;
                }

                if (totalShippingDiscount > shippingCoupon.MaxDiscountAmount)
                {
                    totalShippingDiscount = shippingCoupon.MaxDiscountAmount;
                }

                foreach (var group in eligibleGroups)
                {
                    if (!shopShippingFees.TryGetValue(group.Key, out var shopFee))
                        continue;

                    var shopDiscount =
                        totalShippingDiscount * (shopFee / categorySpecificShippingFee);
                    shippingDiscounts.Add(group.Key, shopDiscount);
                }

                return shippingDiscounts;
            }

            decimal totalShippingFee = shopShippingFees.Values.Sum();
            decimal totalDiscount;

            if (shippingCoupon.DiscountType == CouponDiscountType.Percent)
            {
                totalDiscount = totalShippingFee * (shippingCoupon.Value / 100);
            }
            else
            {
                totalDiscount =
                    shippingCoupon.Value <= totalShippingFee
                        ? shippingCoupon.Value
                        : totalShippingFee;
            }

            if (totalDiscount > shippingCoupon.MaxDiscountAmount)
            {
                totalDiscount = shippingCoupon.MaxDiscountAmount;
            }

            foreach (var shop in shopShippingFees.Keys)
            {
                var shopFee = shopShippingFees[shop];
                var shopDiscount = totalDiscount * (shopFee / totalShippingFee);
                shippingDiscounts.Add(shop, shopDiscount);
            }

            return shippingDiscounts;
        }

        private Dictionary<User, decimal> CalculateProductDiscounts(
            Coupon? productCoupon,
            Dictionary<User, List<CartItem>> groupedCartItems,
            Dictionary<User, int> shopSubtotals
        )
        {
            if (productCoupon == null)
                return new Dictionary<User, decimal>();

            Dictionary<User, decimal> productDiscounts = new();

            decimal productDiscount;
            if (productCoupon is { Categories.Count: > 0 })
            {
                // flatten the grouped cart items then filter by product coupon categories
                var items = groupedCartItems
                    .SelectMany(g => g.Value)
                    .Where(item =>
                        item.Product.Subcategories.Any(subcategory =>
                            productCoupon.Categories.Any(cc => cc.Id == subcategory.CategoryId)
                        )
                    )
                    .ToList();

                if (items.Count == 0)
                    return productDiscounts;

                decimal categorySpecificSubtotal = items.Sum(item =>
                    (int)
                        Math.Ceiling(
                            item.Product.Discounts.Where(d =>
                                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                                )
                                .OrderBy(d => d.DiscountPrice)
                                .Select(d => (decimal?)d.DiscountPrice)
                                .FirstOrDefault() ?? item.Product.RegularPrice
                        ) * item.Quantity
                );

                if (productCoupon.DiscountType == CouponDiscountType.Percent)
                {
                    productDiscount = categorySpecificSubtotal * (productCoupon.Value / 100);
                }
                else
                {
                    productDiscount =
                        productCoupon.Value <= categorySpecificSubtotal
                            ? productCoupon.Value
                            : categorySpecificSubtotal;
                }

                if (productDiscount > productCoupon.MaxDiscountAmount)
                {
                    productDiscount = productCoupon.MaxDiscountAmount;
                }

                var eligibleGroups = groupedCartItems.Where(group =>
                    group.Value.Any(item =>
                        item.Product.Subcategories.Any(subcategory =>
                            productCoupon.Categories.Any(cc => cc.Id == subcategory.CategoryId)
                        )
                    )
                );

                foreach (var group in eligibleGroups)
                {
                    var groupCategorySpecificSubtotal = group
                        .Value.Where(item =>
                            item.Product.Subcategories.Any(subcategory =>
                                productCoupon.Categories.Any(cc => cc.Id == subcategory.CategoryId)
                            )
                        )
                        .Sum(item =>
                            (int)Math.Ceiling(GetProductPrice(item.Product) * item.Quantity)
                        );

                    var discount =
                        productDiscount
                        * (groupCategorySpecificSubtotal / categorySpecificSubtotal);
                    productDiscounts.Add(group.Key, discount);
                }

                return productDiscounts;
            }

            var subtotal = shopSubtotals.Values.Sum();
            if (productCoupon.DiscountType == CouponDiscountType.Percent)
            {
                productDiscount = subtotal * (productCoupon.Value / 100);
            }
            else
            {
                productDiscount = productCoupon.Value <= subtotal ? productCoupon.Value : subtotal;
            }

            if (productDiscount > productCoupon.MaxDiscountAmount)
            {
                productDiscount = productCoupon.MaxDiscountAmount;
            }

            foreach (var group in groupedCartItems)
            {
                var groupSubtotal = shopSubtotals[group.Key];
                var discount = productDiscount * ((decimal)groupSubtotal / subtotal);
                productDiscounts.Add(group.Key, discount);
            }

            return productDiscounts;
        }

        private async Task<Result<int>> GetShippingFeeAsync(
            Command request,
            UserAddress shopAddress,
            List<CartItem> items,
            Ward shippingWard
        )
        {
            var shippingRequest = new CreateShippingRequest
            {
                PaymentTypeId = 1,
                RequiredNote = "CHOXEMHANGKHONGTHU",
                FromName = shopAddress.Name,
                FromPhone = shopAddress.PhoneNumber,
                FromAddress = shopAddress.Address,
                FromWardName = shopAddress.Ward.Name,
                FromDistrictName = shopAddress.Ward.District.Name,
                FromProvinceName = shopAddress.Ward.District.Province.Name,
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
                return Result<int>.Success((int)fee);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message, 500);
            }
        }

        private decimal GetProductPrice(Product product)
        {
            var discountPrice = product
                .Discounts.Where(d =>
                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                )
                .OrderBy(d => d.DiscountPrice)
                .Select(d => (decimal?)d.DiscountPrice)
                .FirstOrDefault();

            return discountPrice ?? product.RegularPrice;
        }
    }
}
