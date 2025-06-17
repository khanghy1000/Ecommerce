using AutoMapper;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Application.Coupons.Queries;
using Ecommerce.Tests.Application.Coupons.Helpers;
using Shouldly;

namespace Ecommerce.Tests.Application.Coupons.Queries;

public class ListCouponsTests
{
    private readonly IMapper _mapper = CouponTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CouponTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task ListCoupons_ShouldReturnAllCoupons()
    {
        // Arrange
        var query = new ListCoupons.Query();
        var handler = new ListCoupons.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(4); // From test data in CouponTestHelper

        // Verify all expected coupons are returned
        var couponCodes = result.Value.Select(c => c.Code).ToList();
        couponCodes.ShouldContain("WELCOME10");
        couponCodes.ShouldContain("FREESHIP");
        couponCodes.ShouldContain("EXPIRED");
        couponCodes.ShouldContain("INACTIVE");
    }

    [Fact]
    public async Task ListCoupons_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var query = new ListCoupons.Query();
        var handler = new ListCoupons.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var welcome10Coupon = result.Value.FirstOrDefault(c => c.Code == "WELCOME10");
        welcome10Coupon.ShouldNotBeNull();
        welcome10Coupon.Type.ShouldBe(Domain.CouponType.Product);
        welcome10Coupon.Active.ShouldBeTrue();
        welcome10Coupon.DiscountType.ShouldBe(Domain.CouponDiscountType.Percent);
        welcome10Coupon.Value.ShouldBe(10);
        welcome10Coupon.MinOrderValue.ShouldBe(100);
        welcome10Coupon.MaxDiscountAmount.ShouldBe(50);
        welcome10Coupon.AllowMultipleUse.ShouldBeTrue();
        welcome10Coupon.MaxUseCount.ShouldBe(5);
        welcome10Coupon.UsedCount.ShouldBe(0);

        // Check categories
        welcome10Coupon.Categories.Count.ShouldBe(1);
        welcome10Coupon.Categories.First().Id.ShouldBe(1);
        welcome10Coupon.Categories.First().Name.ShouldBe("Electronics");
    }
}
