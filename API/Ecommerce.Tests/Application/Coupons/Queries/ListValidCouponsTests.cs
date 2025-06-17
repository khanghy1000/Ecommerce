using AutoMapper;
using Ecommerce.Application.Coupons.Queries;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Coupons.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Coupons.Queries;

public class ListValidCouponsTests
{
    private readonly IMapper _mapper = CouponTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CouponTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();

    public ListValidCouponsTests()
    {
        // Set up a fake user
        var user = new User { Id = "test-user-id", UserName = "testuser" };
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(user);
    }

    [Fact]
    public async Task ListValidCoupons_ShouldReturnOnlyValidCoupons()
    {
        // Arrange
        var query = new ListValidCoupons.Query
        {
            OrderSubtotal = 200,
            ProductCategoryIds = new List<int> { 1 }, // Electronics category
        };
        var handler = new ListValidCoupons.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Should only include active and non-expired coupons (WELCOME10 and FREESHIP)
        result.Value.Count.ShouldBe(2);
        var couponCodes = result.Value.Select(c => c.Code).ToList();
        couponCodes.ShouldContain("WELCOME10");
        couponCodes.ShouldContain("FREESHIP");
        couponCodes.ShouldNotContain("EXPIRED");
        couponCodes.ShouldNotContain("INACTIVE");
    }

    [Fact]
    public async Task ListValidCoupons_ShouldFilterByOrderSubtotal()
    {
        // Arrange
        var query = new ListValidCoupons.Query
        {
            OrderSubtotal = 150, // Less than FREESHIP min order value (200)
            ProductCategoryIds = new List<int> { 1 }, // Electronics category
        };
        var handler = new ListValidCoupons.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Should only include WELCOME10 as FREESHIP requires order subtotal of 200
        result.Value.Count.ShouldBe(1);
        result.Value.First().Code.ShouldBe("WELCOME10");
    }

    [Fact]
    public async Task ListValidCoupons_ShouldFilterByProductCategories()
    {
        // Arrange
        // First, add a category to the FREESHIP coupon
        var freeship = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "FREESHIP");

        freeship.ShouldNotBeNull();
        var clothingCategory = await _dbContext.Categories.FindAsync(2);
        clothingCategory.ShouldNotBeNull();
        freeship.Categories.Add(clothingCategory);
        await _dbContext.SaveChangesAsync();

        var query = new ListValidCoupons.Query
        {
            OrderSubtotal = 250,
            ProductCategoryIds = new List<int> { 2 }, // Clothing category
        };
        var handler = new ListValidCoupons.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Should only include FREESHIP as it's associated with Clothing category
        result.Value.Count.ShouldBe(1);
        result.Value.First().Code.ShouldBe("FREESHIP");
    }

    [Fact]
    public async Task ListValidCoupons_ShouldExcludeUsedNonMultipleUseCoupons()
    {
        // Arrange
        // First create a sales order that used the FREESHIP coupon
        var user = await _userAccessor.GetUserAsync();
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == "FREESHIP");
        coupon.ShouldNotBeNull();

        var order = new SalesOrder
        {
            Id = 100,
            UserId = user.Id,
            User = user,
            Status = SalesOrderStatus.PendingConfirmation,
            ShippingName = "Test User",
            ShippingPhone = "1234567890",
            ShippingAddress = "Test Address",
            Subtotal = 200,
            Total = 250,
            Coupons = new List<Coupon> { coupon },
            ShippingFee = 50,
            ShippingWardId = 0,
            PaymentMethod = PaymentMethod.Cod,
        };

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        var query = new ListValidCoupons.Query
        {
            OrderSubtotal = 250,
            ProductCategoryIds = new List<int> { 1, 2 }, // Both categories
        };
        var handler = new ListValidCoupons.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Should only include WELCOME10 as FREESHIP was used and doesn't allow multiple use
        result.Value.Count.ShouldBe(1);
        result.Value.First().Code.ShouldBe("WELCOME10");
    }

    [Fact]
    public async Task ListValidCoupons_ShouldIncludeUsedMultipleUseCoupons()
    {
        // Arrange
        // First create a sales order that used the WELCOME10 coupon
        var user = await _userAccessor.GetUserAsync();
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == "WELCOME10");
        coupon.ShouldNotBeNull();

        var order = new SalesOrder
        {
            Id = 200,
            UserId = user.Id,
            User = user,
            Status = SalesOrderStatus.PendingConfirmation,
            ShippingName = "Test User",
            ShippingPhone = "1234567890",
            ShippingAddress = "Test Address",
            Subtotal = 200,
            Total = 250,
            Coupons = new List<Coupon> { coupon },
            ShippingFee = 50,
            ShippingWardId = 0,
            PaymentMethod = PaymentMethod.Cod,
        };

        _dbContext.SalesOrders.Add(order);
        await _dbContext.SaveChangesAsync();

        var query = new ListValidCoupons.Query
        {
            OrderSubtotal = 150,
            ProductCategoryIds = new List<int> { 1 }, // Electronics category
        };
        var handler = new ListValidCoupons.Handler(_dbContext, _mapper, _userAccessor);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Should still include WELCOME10 as it allows multiple use
        result.Value.Count.ShouldBe(1);
        result.Value.First().Code.ShouldBe("WELCOME10");
    }
}
