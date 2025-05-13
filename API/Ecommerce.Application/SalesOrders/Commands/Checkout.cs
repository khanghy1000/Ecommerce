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

            var cartItems = await GetCartItemsAsync(
                user.Id,
                request.CheckoutRequestDto.ProductIds,
                cancellationToken
            );

            if (cartItems.Count == 0)
                return Result<CheckoutResponseDto>.Failure("Cart is empty", 400);

            if (cartItems.Any(ci => ci.Product.Active))
                return Result<CheckoutResponseDto>.Failure("Some products are not available", 400);

            if (cartItems.Any(ci => ci.Quantity > ci.Product.Quantity))
                return Result<CheckoutResponseDto>.Failure("Some products are out of stock", 400);

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
                    return Result<CheckoutResponseDto>.Failure("Shop address not found", 400);

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
                    return Result<CheckoutResponseDto>.Failure(
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

            if (!string.IsNullOrEmpty(request.CheckoutRequestDto.ProductCouponCode))
            {
                var productCouponResult = await mediator.Send(
                    new ValidateCoupon.Command
                    {
                        CouponCode = request.CheckoutRequestDto.ProductCouponCode,
                        CouponType = CouponType.Product,
                        OrderSubtotal = shopSubtotals.Values.Sum(),
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
                        OrderSubtotal = shopSubtotals.Values.Sum(),
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

            if (productCoupon != null)
            {
                productCoupon.UsedCount += 1;
                dbContext.Update(productCoupon);
            }

            if (shippingCoupon != null)
            {
                shippingCoupon.UsedCount += 1;
                dbContext.Update(shippingCoupon);
            }

            // Create sales orders for each shop
            var createdSalesOrders = new List<SalesOrder>();

            foreach (var (shop, items) in groupedCartItems)
            {
                var shopProductDiscount = productDiscounts.GetValueOrDefault(shop);
                var shopShippingDiscount = shippingDiscounts.GetValueOrDefault(shop);

                var createOrderResult = await CreateOrder(
                    request,
                    items,
                    productCoupon,
                    shippingCoupon,
                    user,
                    shopSubtotals[shop],
                    shopShippingFees[shop],
                    shopProductDiscount,
                    shopShippingDiscount,
                    cancellationToken
                );

                if (!createOrderResult.IsSuccess)
                    return Result<CheckoutResponseDto>.Failure(
                        createOrderResult.Error!,
                        createOrderResult.Code
                    );

                createdSalesOrders.Add(createOrderResult.Value!);
            }

            // update product quantity
            foreach (var item in cartItems)
            {
                var product = item.Product;
                product.Quantity -= item.Quantity;
                if (product.Quantity < 0)
                    product.Quantity = 0;
                dbContext.Products.Update(product);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

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
            List<CartItem> items,
            Coupon? productCoupon,
            Coupon? shippingCoupon,
            User user,
            int subtotal,
            int shippingFee,
            decimal productDiscount,
            decimal shippingDiscount,
            CancellationToken cancellationToken
        )
        {
            var discountedSubtotal = subtotal - (int)Math.Ceiling(productDiscount);
            var discountedShippingFee = shippingFee - (int)Math.Ceiling(shippingDiscount);
            var total = discountedSubtotal + discountedShippingFee;

            var order = new SalesOrder
            {
                UserId = user.Id,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                ProductDiscountAmount = (int)Math.Ceiling(productDiscount),
                ShippingDiscountAmount = (int)Math.Ceiling(shippingDiscount),
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
            }
            if (shippingCoupon != null)
            {
                order.Coupons.Add(shippingCoupon);
            }

            dbContext.SalesOrders.Add(order);

            var orderProducts = new List<OrderProduct>();
            foreach (var item in items)
            {
                var itemPrice = GetProductPrice(item.Product);

                var orderProduct = new OrderProduct
                {
                    Name = item.Product.Name,
                    Price = itemPrice,
                    Quantity = item.Quantity,
                    Subtotal = (itemPrice * item.Quantity),
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
                    (int)Math.Ceiling(GetProductPrice(item.Product) * item.Quantity)
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

        private async Task<List<CartItem>> GetCartItemsAsync(
            string userId,
            List<int> productIds,
            CancellationToken cancellationToken
        )
        {
            var cartItems = await dbContext
                .CartItems.Include(ci => ci.Product)
                .ThenInclude(p => p.Shop)
                .Include(ci => ci.Product)
                .ThenInclude(product => product.Discounts)
                .Include(ci => ci.Product.Subcategories)
                .ThenInclude(s => s.Category)
                .Where(ci => ci.UserId == userId && productIds.Contains(ci.ProductId))
                .ToListAsync(cancellationToken);
            return cartItems;
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
