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
}
