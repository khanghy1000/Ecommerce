using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Application.Reviews.Helpers;

public static class ReviewTestHelper
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
        if (await dbContext.ProductReviews.AnyAsync())
            return dbContext;

        // Create test users
        var buyer = new User { Id = "user1", DisplayName = "Test Buyer" };
        var shop = new User { Id = "shop1", DisplayName = "Shop Test" };

        dbContext.Users.Add(buyer);
        dbContext.Users.Add(shop);

        // Create test products
        var product1 = new Product
        {
            Id = 1,
            Name = "Product 1",
            Description = "Description 1",
            RegularPrice = 100,
            Quantity = 10,
            Active = true,
            Length = 10,
            Width = 10,
            Height = 10,
            Weight = 10,
            ShopId = shop.Id,
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "Product 2",
            Description = "Description 2",
            RegularPrice = 200,
            Quantity = 20,
            Active = true,
            Length = 20,
            Width = 20,
            Height = 20,
            Weight = 20,
            ShopId = shop.Id,
        };

        dbContext.Products.Add(product1);
        dbContext.Products.Add(product2);

        // Create sales order for the user (representing purchased products)
        var salesOrder = new SalesOrder
        {
            Id = 1,
            UserId = buyer.Id,
            Subtotal = 300,
            ShippingFee = 10,
            Total = 310,
            ShippingName = "Test Buyer",
            ShippingPhone = "1234567890",
            ShippingAddress = "123 Test St",
            ShippingWardId = 1,
            PaymentMethod = PaymentMethod.Cod,
            Status = SalesOrderStatus.Delivered,
        };

        dbContext.SalesOrders.Add(salesOrder);

        // Create order products (items the user has purchased)
        var orderProduct1 = new OrderProduct
        {
            Id = 1,
            Name = "Product 1",
            Price = 100,
            Quantity = 1,
            Subtotal = 100,
            OrderId = salesOrder.Id,
            ProductId = product1.Id,
        };

        var orderProduct2 = new OrderProduct
        {
            Id = 2,
            Name = "Product 2",
            Price = 200,
            Quantity = 1,
            Subtotal = 200,
            OrderId = salesOrder.Id,
            ProductId = product2.Id,
        };

        dbContext.OrderProducts.Add(orderProduct1);
        dbContext.OrderProducts.Add(orderProduct2);

        // Create test reviews
        for (var i = 1; i <= 5; i++)
        {
            dbContext.ProductReviews.Add(
                new ProductReview
                {
                    Id = i,
                    ProductId = 1,
                    UserId = buyer.Id,
                    Rating = i,
                    Review = $"Review {i} for product 1",
                }
            );
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    public static IUserAccessor GetUserAccessor(string userId = "user1")
    {
        var userAccessor = A.Fake<IUserAccessor>();
        A.CallTo(() => userAccessor.GetUserId()).Returns(userId);

        // Mock GetUserAsync to return a user with the specified userId
        A.CallTo(() => userAccessor.GetUserAsync())
            .Returns(Task.FromResult(new User { Id = userId, DisplayName = "Test User" }));

        return userAccessor;
    }
}
