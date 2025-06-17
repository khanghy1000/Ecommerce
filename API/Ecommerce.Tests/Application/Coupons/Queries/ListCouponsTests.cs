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
}
