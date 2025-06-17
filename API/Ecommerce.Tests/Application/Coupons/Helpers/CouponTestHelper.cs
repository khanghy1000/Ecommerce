using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Application.Coupons.Helpers;

public static class CouponTestHelper
{
    public static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
        return config.CreateMapper();
    }

    public static async Task<TestAppDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestAppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        if (await dbContext.Coupons.AnyAsync())
            return dbContext;

        // Add some test categories
        var category1 = new Category { Id = 1, Name = "Electronics" };
        var category2 = new Category { Id = 2, Name = "Clothing" };

        dbContext.Categories.Add(category1);
        dbContext.Categories.Add(category2);

        // Add some test coupons
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Code = "WELCOME10",
                Active = true,
                StartTime = DateTime.UtcNow.AddDays(-10),
                EndTime = DateTime.UtcNow.AddDays(30),
                Type = CouponType.Product,
                DiscountType = CouponDiscountType.Percent,
                Value = 10,
                MinOrderValue = 100,
                MaxDiscountAmount = 50,
                AllowMultipleUse = true,
                MaxUseCount = 5,
                UsedCount = 0,
                Categories = new List<Category> { category1 },
            },
            new Coupon
            {
                Code = "FREESHIP",
                Active = true,
                StartTime = DateTime.UtcNow.AddDays(-5),
                EndTime = DateTime.UtcNow.AddDays(5),
                Type = CouponType.Shipping,
                DiscountType = CouponDiscountType.Amount,
                Value = 20,
                MinOrderValue = 200,
                MaxDiscountAmount = 20,
                AllowMultipleUse = false,
                MaxUseCount = 1,
                UsedCount = 0,
            },
            new Coupon
            {
                Code = "EXPIRED",
                Active = true,
                StartTime = DateTime.UtcNow.AddDays(-30),
                EndTime = DateTime.UtcNow.AddDays(-15),
                Type = CouponType.Product,
                DiscountType = CouponDiscountType.Percent,
                Value = 15,
                MinOrderValue = 150,
                MaxDiscountAmount = 100,
                AllowMultipleUse = true,
                MaxUseCount = 10,
                UsedCount = 3,
            },
            new Coupon
            {
                Code = "INACTIVE",
                Active = false,
                StartTime = DateTime.UtcNow.AddDays(-10),
                EndTime = DateTime.UtcNow.AddDays(30),
                Type = CouponType.Product,
                DiscountType = CouponDiscountType.Percent,
                Value = 25,
                MinOrderValue = 200,
                MaxDiscountAmount = 150,
                AllowMultipleUse = true,
                MaxUseCount = 10,
                UsedCount = 0,
            },
        };

        foreach (var coupon in coupons)
        {
            dbContext.Coupons.Add(coupon);
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }
}
