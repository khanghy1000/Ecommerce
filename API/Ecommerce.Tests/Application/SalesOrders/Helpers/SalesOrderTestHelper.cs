using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Application.SalesOrders.Helpers;

public static class SalesOrderTestHelper
{
    public static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
        return config.CreateMapper();
    }

    public static IUserAccessor GetFakeUserAccessor(User mockUser, string roleType)
    {
        var userAccessor = A.Fake<IUserAccessor>();
        A.CallTo(() => userAccessor.GetUserAsync()).Returns(mockUser);
        A.CallTo(() => userAccessor.GetUserId()).Returns(mockUser.Id);
        A.CallTo(() => userAccessor.GetUserRoles()).Returns(new[] { roleType });
        return userAccessor;
    }

    public static async Task<TestAppDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestAppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        if (await dbContext.SalesOrders.AnyAsync())
            return dbContext;

        var buyer = new User { Id = Guid.NewGuid().ToString(), DisplayName = "Test Buyer" };
        var shop = new User { Id = Guid.NewGuid().ToString(), DisplayName = "Test Shop" };
        var province = new Province { Id = 1, Name = "Test Province" };
        var district = new District
        {
            Id = 1,
            Name = "Test District",
            ProvinceId = 1,
        };
        var ward = new Ward
        {
            Id = 1,
            Name = "Test Ward",
            DistrictId = 1,
        };

        dbContext.Users.AddRange(new[] { buyer, shop });
        dbContext.Provinces.Add(province);
        dbContext.Districts.Add(district);
        dbContext.Wards.Add(ward);

        // Add shop address
        var shopAddress = new UserAddress
        {
            Id = 1,
            UserId = shop.Id,
            Address = "Shop Test Address",
            WardId = ward.Id,
            IsDefault = true,
            Name = "Shop Owner",
            PhoneNumber = "0987654321",
        };

        dbContext.UserAddresses.Add(shopAddress);

        // Add some products
        for (var i = 1; i <= 5; i++)
        {
            dbContext.Products.Add(
                new Product
                {
                    Id = i,
                    Name = $"Product {i}",
                    Description = $"Description {i}",
                    RegularPrice = 100 + i * 10,
                    Quantity = 20,
                    Active = true,
                    Length = 10,
                    Width = 10,
                    Height = 10,
                    Weight = 10,
                    ShopId = shop.Id,
                    Shop = shop,
                }
            );
        }

        // Create sales orders with different statuses
        var statuses = Enum.GetValues<SalesOrderStatus>();

        for (var i = 1; i <= 5; i++)
        {
            var order = new SalesOrder
            {
                Id = i,
                UserId = buyer.Id,
                User = buyer,
                OrderTime = DateTime.UtcNow.AddDays(-i),
                Subtotal = 150.0m,
                ShippingFee = 10,
                Total = 160.0m,
                ShippingName = "Test Receiver",
                ShippingPhone = "1234567890",
                ShippingAddress = "Test Address",
                ShippingWardId = ward.Id,
                ShippingWard = ward,
                PaymentMethod = i % 2 == 0 ? PaymentMethod.Cod : PaymentMethod.Vnpay,
                Status = statuses[i % statuses.Length],
            };

            // Add order products
            dbContext.OrderProducts.Add(
                new OrderProduct
                {
                    ProductId = i,
                    OrderId = i,
                    Quantity = 1,
                    Price = 150,
                    Name = $"Product {i}",
                    Subtotal = 150,
                }
            );

            dbContext.SalesOrders.Add(order);
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    public static IShippingService GetFakeShippingService()
    {
        var shippingService = A.Fake<IShippingService>();
        return shippingService;
    }

    public static IPaymentService GetFakePaymentService()
    {
        var paymentService = A.Fake<IPaymentService>();
        return paymentService;
    }
}
