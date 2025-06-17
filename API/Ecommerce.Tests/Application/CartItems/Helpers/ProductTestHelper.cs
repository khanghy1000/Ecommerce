using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Application.CartItems.Helpers;

public static class CartTestHelper
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

        if (await dbContext.Products.AnyAsync())
            return dbContext;

        // Setup test user
        var user = new User
        {
            Id = "test-user-id",
            DisplayName = "Test User",
            Email = "test@example.com",
        };

        // Setup test shop
        var shop = new User
        {
            Id = "test-shop-id",
            DisplayName = "Test Shop",
            Email = "shop@example.com",
        };

        dbContext.Users.Add(user);
        dbContext.Users.Add(shop);

        // Setup test products
        for (var i = 0; i < 5; i++)
        {
            dbContext.Products.Add(
                new Product
                {
                    Id = i + 1,
                    Name = $"Test Product {i + 1}",
                    Description = $"Description for test product {i + 1}",
                    RegularPrice = 100 + (i * 10),
                    Quantity = 20,
                    Active = true,
                    Length = 10,
                    Width = 10,
                    Height = 10,
                    Weight = 10,
                    ShopId = shop.Id,
                }
            );
        }

        // Setup test cart items
        dbContext.CartItems.Add(
            new CartItem
            {
                UserId = user.Id,
                ProductId = 1,
                Quantity = 2,
            }
        );

        await dbContext.SaveChangesAsync();
        return dbContext;
    }
}
