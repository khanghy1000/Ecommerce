using AutoMapper;
using Ecommerce.Application.Coupons.Commands;
using Ecommerce.Tests.Application.Coupons.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Coupons.Commands;

public class DeleteCouponTests
{
    private readonly IMapper _mapper = CouponTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CouponTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task DeleteCoupon_ShouldDeleteCoupon_WhenValidCode()
    {
        // Arrange
        var command = new DeleteCoupon.Command { Code = "WELCOME10" };
        var handler = new DeleteCoupon.Handler(_dbContext);

        // Verify the coupon exists before deletion
        var couponBeforeDeletion = await _dbContext.Coupons.FindAsync("WELCOME10");
        couponBeforeDeletion.ShouldNotBeNull();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Unit.Value);

        // Verify the coupon was removed from the database
        var couponAfterDeletion = await _dbContext.Coupons.FindAsync("WELCOME10");
        couponAfterDeletion.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteCoupon_ShouldReturnFailure_WhenCouponNotFound()
    {
        // Arrange
        var command = new DeleteCoupon.Command { Code = "NONEXISTENT" };
        var handler = new DeleteCoupon.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Coupon not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task DeleteCoupon_ShouldRemoveCouponWithCategories()
    {
        // Arrange
        // First ensure coupon has categories
        var coupon = await _dbContext
            .Coupons.Include(c => c.Categories)
            .FirstOrDefaultAsync(c => c.Code == "WELCOME10");

        coupon.ShouldNotBeNull();
        coupon.Categories.Count.ShouldBeGreaterThan(0);

        var command = new DeleteCoupon.Command { Code = "WELCOME10" };
        var handler = new DeleteCoupon.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the coupon was removed from the database
        var couponAfterDeletion = await _dbContext.Coupons.FindAsync("WELCOME10");
        couponAfterDeletion.ShouldBeNull();
    }
}
