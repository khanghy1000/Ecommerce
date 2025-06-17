using Ecommerce.Application.Coupons.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Coupons.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Coupons.Commands;

public class ValidateCouponTests
{
    private readonly TestAppDbContext _dbContext = CouponTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();

    public ValidateCouponTests()
    {
        // Set up a fake user
        var user = new User { Id = "test-user-id", UserName = "testuser" };
        A.CallTo(() => _userAccessor.GetUserAsync()).Returns(user);
    }

    [Fact]
    public async Task ValidateCoupon_ShouldReturnCoupon_WhenValidRequest()
    {
        // Arrange
        var command = new ValidateCoupon.Command
        {
            CouponCode = "WELCOME10",
            CouponType = CouponType.Product,
            OrderSubtotal = 150,
            ProductCategoryIds = new List<int> { 1 }, // Electronics category
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Code.ShouldBe("WELCOME10");
    }

    [Fact]
    public async Task ValidateCoupon_ShouldReturnFailure_WhenCouponNotFound()
    {
        // Arrange
        var command = new ValidateCoupon.Command
        {
            CouponCode = "NONEXISTENT",
            CouponType = CouponType.Product,
            OrderSubtotal = 150,
            ProductCategoryIds = new List<int> { 1 },
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Invalid or expired coupon");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task ValidateCoupon_ShouldReturnFailure_WhenCouponIsExpired()
    {
        // Arrange
        var command = new ValidateCoupon.Command
        {
            CouponCode = "EXPIRED",
            CouponType = CouponType.Product,
            OrderSubtotal = 150,
            ProductCategoryIds = new List<int> { 1 },
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Invalid or expired coupon");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task ValidateCoupon_ShouldReturnFailure_WhenCouponIsInactive()
    {
        // Arrange
        var command = new ValidateCoupon.Command
        {
            CouponCode = "INACTIVE",
            CouponType = CouponType.Product,
            OrderSubtotal = 150,
            ProductCategoryIds = new List<int> { 1 },
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Invalid or expired coupon");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task ValidateCoupon_ShouldReturnFailure_WhenOrderSubtotalIsTooLow()
    {
        // Arrange
        var command = new ValidateCoupon.Command
        {
            CouponCode = "WELCOME10",
            CouponType = CouponType.Product,
            OrderSubtotal = 50, // Below min order value of 100
            ProductCategoryIds = new List<int> { 1 },
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Order subtotal must be at least 100");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task ValidateCoupon_ShouldReturnFailure_WhenCategoryDoesNotMatch()
    {
        // Arrange
        var command = new ValidateCoupon.Command
        {
            CouponCode = "WELCOME10",
            CouponType = CouponType.Product,
            OrderSubtotal = 150,
            ProductCategoryIds = new List<int> { 3 }, // Not matching any category in the coupon
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Coupon is not applicable to the selected products");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task ValidateCoupon_ShouldSucceed_WhenCouponHasNoCategories()
    {
        // Arrange
        // First, remove categories from FREESHIP coupon
        var coupon = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "FREESHIP");

        coupon.ShouldNotBeNull();
        coupon.Categories.Clear();
        await _dbContext.SaveChangesAsync();

        var command = new ValidateCoupon.Command
        {
            CouponCode = "FREESHIP",
            CouponType = CouponType.Shipping,
            OrderSubtotal = 250,
            ProductCategoryIds = new List<int> { 3 }, // Any category should work as coupon has no category restrictions
        };
        var handler = new ValidateCoupon.Handler(_dbContext, _userAccessor);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Code.ShouldBe("FREESHIP");
    }
}
