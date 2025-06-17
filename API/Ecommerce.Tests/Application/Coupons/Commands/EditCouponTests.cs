using AutoMapper;
using Ecommerce.Application.Coupons.Commands;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Coupons.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Coupons.Commands;

public class EditCouponTests
{
    private readonly IMapper _mapper = CouponTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CouponTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task EditCoupon_ShouldUpdateCoupon_WhenValidRequest()
    {
        // Arrange
        var editCouponDto = new EditCouponRequestDto
        {
            Active = true,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(60),
            Type = CouponType.Product,
            DiscountType = CouponDiscountType.Percent,
            Value = 15,
            MinOrderValue = 150,
            MaxDiscountAmount = 75,
            AllowMultipleUse = false,
            MaxUseCount = 1,
            CategoryIds = new List<int> { 2 }, // Change to Clothing category
        };

        var command = new EditCoupon.Command { Code = "WELCOME10", CouponRequest = editCouponDto };
        var handler = new EditCoupon.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Code.ShouldBe("WELCOME10");
        result.Value.Value.ShouldBe(15);
        result.Value.MinOrderValue.ShouldBe(150);
        result.Value.MaxDiscountAmount.ShouldBe(75);
        result.Value.AllowMultipleUse.ShouldBe(false);
        result.Value.MaxUseCount.ShouldBe(1);
        result.Value.Categories.Count.ShouldBe(1);
        result.Value.Categories.First().Id.ShouldBe(2);

        // Verify the coupon was updated in the database
        var couponFromDb = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "WELCOME10");
        couponFromDb.ShouldNotBeNull();
        couponFromDb.Value.ShouldBe(15);
        couponFromDb.MinOrderValue.ShouldBe(150);
        couponFromDb.MaxDiscountAmount.ShouldBe(75);
        couponFromDb.Categories.Count.ShouldBe(1);
        couponFromDb.Categories.First().Id.ShouldBe(2);
    }

    [Fact]
    public async Task EditCoupon_ShouldRemoveAllCategories_WhenCategoryIdsIsEmpty()
    {
        // Arrange
        var editCouponDto = new EditCouponRequestDto
        {
            Active = true,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(30),
            Type = CouponType.Product,
            DiscountType = CouponDiscountType.Percent,
            Value = 15,
            MinOrderValue = 100,
            MaxDiscountAmount = 50,
            AllowMultipleUse = true,
            MaxUseCount = 5,
            CategoryIds = new List<int>(), // Empty list
        };

        var command = new EditCoupon.Command { Code = "WELCOME10", CouponRequest = editCouponDto };
        var handler = new EditCoupon.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Categories.Count.ShouldBe(0);

        // Verify the categories were removed in the database
        var couponFromDb = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "WELCOME10");
        couponFromDb.ShouldNotBeNull();
        couponFromDb.Categories.Count.ShouldBe(0);
    }
}
