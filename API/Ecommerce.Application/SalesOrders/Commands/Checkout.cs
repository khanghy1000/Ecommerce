using System.Diagnostics.CodeAnalysis;
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
        IPaymentService paymentService,
        IMediator mediator
    ) : IRequestHandler<Command, Result<CheckoutResponseDto>>
    {
        public async Task<Result<CheckoutResponseDto>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
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

            var cartItems = await GetCartItemsAsync(user.Id, cancellationToken);

            if (cartItems.Count == 0)
                return Result<CheckoutResponseDto>.Failure("Cart is empty", 400);

            // Calculate subtotal for all items for checking coupon minimum order value
            var checkoutSubTotal = (int)
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

            var productCategoryIds = cartItems
                .SelectMany(ci => ci.Product.Subcategories.Select(s => s.CategoryId))
                .Distinct()
                .ToList();

            Coupon? productCoupon = null;
            Coupon? shippingCoupon = null;

            if (!string.IsNullOrEmpty(request.CheckoutRequestDto.ProductCouponCode))
            {
                var productCouponResult = await mediator.Send(
                    new ValidateCoupon.Command
                    {
                        CouponCode = request.CheckoutRequestDto.ProductCouponCode,
                        CouponType = CouponType.Product,
                        OrderSubtotal = checkoutSubTotal,
                        ProductCategoryIds = productCategoryIds,
                    },
                    cancellationToken
                );

                if (!productCouponResult.IsSuccess)
                    return Result<CheckoutResponseDto>.Failure(
                        productCouponResult.Error!,
                        productCouponResult.Code
                    );
                productCoupon = productCouponResult.Value;
            }

            if (!string.IsNullOrEmpty(request.CheckoutRequestDto.ShippingCouponCode))
            {
                var shippingCouponResult = await mediator.Send(
                    new ValidateCoupon.Command
                    {
                        CouponCode = request.CheckoutRequestDto.ShippingCouponCode,
                        CouponType = CouponType.Shipping,
                        OrderSubtotal = checkoutSubTotal,
                        ProductCategoryIds = productCategoryIds,
                    },
                    cancellationToken
                );

                if (!shippingCouponResult.IsSuccess)
                    return Result<CheckoutResponseDto>.Failure(
                        shippingCouponResult.Error!,
                        shippingCouponResult.Code
                    );
                shippingCoupon = shippingCouponResult.Value;
            }

            var groupedCartItems = cartItems.GroupBy(ci => ci.Product.Shop).ToList();
            var createdSalesOrders = new List<SalesOrder>();

            // Create order for each shop
            foreach (var groupedCartItem in groupedCartItems)
            {
                var createOrderResult = await CreateOrder(
                    request,
                    groupedCartItem,
                    shippingWard,
                    productCoupon,
                    shippingCoupon,
                    user,
                    cancellationToken
                );
                if (!createOrderResult.IsSuccess)
                    return Result<CheckoutResponseDto>.Failure(
                        createOrderResult.Error!,
                        createOrderResult.Code
                    );
                createdSalesOrders.Add(createOrderResult.Value!);
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
                    (int)createdSalesOrders.Sum(so => so.Total),
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

        private async Task<Result<SalesOrder>> CreateOrder(
            Command request,
            IGrouping<User, CartItem> groupedCartItem,
            Ward shippingWard,
            Coupon? productCoupon,
            Coupon? shippingCoupon,
            User user,
            CancellationToken cancellationToken
        )
        {
            var shop = groupedCartItem.Key;
            var items = groupedCartItem.ToList();

            var subtotal = (int)
                Math.Ceiling(
                    items.Sum(i =>
                        (
                            i.Product.Discounts.Where(d =>
                                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                                )
                                .OrderBy(d => d.DiscountPrice)
                                .Select(d => (decimal?)d.DiscountPrice)
                                .FirstOrDefault() ?? i.Product.RegularPrice
                        ) * i.Quantity
                    )
                );

            var getShippingFeeResult = await GetShippingFeeAsync(
                request,
                groupedCartItem,
                shippingWard
            );

            if (!getShippingFeeResult.IsSuccess)
            {
                return Result<SalesOrder>.Failure(
                    getShippingFeeResult.Error!,
                    getShippingFeeResult.Code
                );
            }
            var shippingFee = getShippingFeeResult.Value;

            var productDiscount = GetProductDiscount(productCoupon, items, subtotal);
            var shippingDiscount = GetShippingDiscount(shippingCoupon, shippingFee);

            var discountedSubtotal = subtotal - (int)Math.Ceiling(productDiscount);
            var discountedShippingFee = (int)Math.Ceiling(shippingFee - shippingDiscount);
            var total = discountedSubtotal + discountedShippingFee;

            var order = new SalesOrder
            {
                UserId = user.Id,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                ProductDiscountAmount = (decimal)Math.Ceiling(productDiscount),
                ShippingDiscountAmount = (decimal)Math.Ceiling(shippingDiscount),
                Total = total,
                ShippingName = request.CheckoutRequestDto.ShippingName,
                ShippingPhone = request.CheckoutRequestDto.ShippingPhone,
                ShippingAddress = request.CheckoutRequestDto.ShippingAddress,
                ShippingWardId = request.CheckoutRequestDto.ShippingWardId,
                PaymentMethod = request.CheckoutRequestDto.PaymentMethod,
                Status =
                    request.CheckoutRequestDto.PaymentMethod == PaymentMethod.Cod
                        ? SalesOrderStatus.PendingConfirmation
                        : SalesOrderStatus.PendingPayment,
                Coupons = new List<Coupon>(),
            };

            if (productCoupon != null)
            {
                order.Coupons.Add(productCoupon);
                productCoupon.UsedCount += 1;
                dbContext.Update(productCoupon);
            }
            if (shippingCoupon != null)
            {
                order.Coupons.Add(shippingCoupon);
                shippingCoupon.UsedCount += 1;
                dbContext.Update(shippingCoupon);
            }

            dbContext.SalesOrders.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            var orderProducts = new List<OrderProduct>();
            foreach (var item in items)
            {
                var itemPrice =
                    item.Product.Discounts.Where(d =>
                            d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                        )
                        .OrderBy(d => d.DiscountPrice)
                        .Select(d => (decimal?)d.DiscountPrice)
                        .FirstOrDefault() ?? item.Product.RegularPrice;

                decimal itemDiscount = 0;

                // Apply item-specific discount if it's a product coupon with category restrictions
                if (
                    productCoupon != null
                    && productCoupon.Categories.Count != 0
                    && item.Product.Subcategories.Any(subcategory =>
                        productCoupon.Categories.Any(cc => cc.Id == subcategory.CategoryId)
                    )
                )
                {
                    if (productCoupon.DiscountType == CouponDiscountType.Percent)
                    {
                        itemDiscount = itemPrice * (productCoupon.Value / 100) * item.Quantity;
                    }
                    else // Amount - distribute proportionally
                    {
                        var proportion = (itemPrice * item.Quantity) / subtotal;
                        itemDiscount = Math.Min(
                            productCoupon.Value * proportion,
                            itemPrice * item.Quantity
                        );
                    }

                    if (itemDiscount > productCoupon.MaxDiscountAmount)
                    {
                        itemDiscount = productCoupon.MaxDiscountAmount;
                    }
                }
                // If product coupon with no category restrictions, apply discount proportionally
                else if (productCoupon is { Categories.Count: 0 })
                {
                    var proportion = (itemPrice * item.Quantity) / subtotal;
                    itemDiscount = productDiscount * proportion;
                }

                var orderProduct = new OrderProduct
                {
                    Name = item.Product.Name,
                    Price = itemPrice,
                    Quantity = item.Quantity,
                    Subtotal = (itemPrice * item.Quantity) - itemDiscount,
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                };

                orderProducts.Add(orderProduct);
            }

            dbContext.OrderProducts.AddRange(orderProducts);
            dbContext.CartItems.RemoveRange(items);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<SalesOrder>.Success(order);
        }

        private decimal GetShippingDiscount(Coupon? shippingCoupon, [DisallowNull] int? fee)
        {
            decimal shippingDiscount = 0;

            if (shippingCoupon is { DiscountType: CouponDiscountType.Percent })
            {
                shippingDiscount = fee.Value * (shippingCoupon.Value / 100);
            }
            else if (shippingCoupon is { DiscountType: CouponDiscountType.Amount })
            {
                shippingDiscount = shippingCoupon.Value;
            }

            if (shippingCoupon != null && shippingDiscount > shippingCoupon.MaxDiscountAmount)
            {
                shippingDiscount = shippingCoupon.MaxDiscountAmount;
            }

            if (shippingCoupon != null && shippingDiscount > fee.Value)
            {
                shippingDiscount = fee.Value;
            }

            return shippingDiscount;
        }

        private decimal GetProductDiscount(
            Coupon? productCoupon,
            List<CartItem> items,
            int subTotal
        )
        {
            if (productCoupon == null)
                return 0;

            decimal productDiscount = 0;

            if (productCoupon is { Categories.Count: > 0 })
            {
                // Apply discount only to products with matching categories
                decimal categorySpecificTotal = 0;
                foreach (var item in items)
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
                    productDiscount = subTotal * (productCoupon.Value / 100);
                }
                else
                {
                    productDiscount =
                        productCoupon.Value <= subTotal ? productCoupon.Value : subTotal;
                }
            }

            if (productDiscount > productCoupon.MaxDiscountAmount)
            {
                productDiscount = productCoupon.MaxDiscountAmount;
            }

            return productDiscount;
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
                .Include(ci => ci.Product)
                .ThenInclude(product => product.Discounts)
                .Include(ci => ci.Product.Subcategories)
                .ThenInclude(s => s.Category)
                .Where(ci => ci.UserId == userId)
                .ToListAsync(cancellationToken);
            return cartItems;
        }

        private async Task<Result<int>> GetShippingFeeAsync(
            Command request,
            IGrouping<User, CartItem> groupedCartItem,
            Ward shippingWard
        )
        {
            var shop = groupedCartItem.Key;
            var items = groupedCartItem.ToList();

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
                var shippingFee = shippingResponse?.Data?.TotalFee;
                return shippingFee == null
                    ? Result<int>.Failure("Shipping fee not found", 400)
                    : Result<int>.Success((int)shippingFee);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message, 500);
            }
        }
    }
}
