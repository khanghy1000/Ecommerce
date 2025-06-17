using AutoMapper;
using Ecommerce.Application.Coupons.Commands;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Coupons.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Coupons.Commands;

public class CreateCouponTests
{
    private readonly IMapper _mapper = CouponTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CouponTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task CreateCoupon_ShouldCreateNewCoupon_WhenValidRequest()
    {
        // Arrange
        var createCouponDto = new CreateCouponRequestDto
        {
            Code = "NEWCOUPON",
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
            CategoryIds = new List<int> { 1 }, // Electronics category
        };

        var command = new CreateCoupon.Command { CouponRequest = createCouponDto };
        var handler = new CreateCoupon.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Code.ShouldBe("NEWCOUPON");
        result.Value.Categories.Count.ShouldBe(1);
        result.Value.Categories.First().Id.ShouldBe(1);

        // Verify the coupon was added to the database
        var couponFromDb = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "NEWCOUPON");
        couponFromDb.ShouldNotBeNull();
        couponFromDb.Categories.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CreateCoupon_ShouldReturnFailure_WhenCodeAlreadyExists()
    {
        // Arrange
        var createCouponDto = new CreateCouponRequestDto
        {
            Code = "WELCOME10", // Existing code from test data
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
        };

        var command = new CreateCoupon.Command { CouponRequest = createCouponDto };
        var handler = new CreateCoupon.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Coupon code already exists");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task CreateCoupon_ShouldCreateCouponWithoutCategories_WhenCategoryIdsIsNull()
    {
        // Arrange
        var createCouponDto = new CreateCouponRequestDto
        {
            Code = "NOCATEGORY",
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
            CategoryIds = null,
        };

        var command = new CreateCoupon.Command { CouponRequest = createCouponDto };
        var handler = new CreateCoupon.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Code.ShouldBe("NOCATEGORY");
        result.Value.Categories.Count.ShouldBe(0);

        // Verify the coupon was added to the database
        var couponFromDb = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "NOCATEGORY");
        couponFromDb.ShouldNotBeNull();
        couponFromDb.Categories.Count.ShouldBe(0);
    }
}
