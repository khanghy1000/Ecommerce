using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Application.Products.Helpers;

public static class ProductTestHelper
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

        var shop = new User { Id = Guid.NewGuid().ToString(), DisplayName = "Shop Test" };

        dbContext.Users.Add(shop);

        for (var i = 0; i < 10; i++)
        {
            dbContext.Products.Add(
                new Product
                {
                    Name = $"Product {i}",
                    Description = $"Description {i}",
                    RegularPrice = 110 + i,
                    Quantity = 10,
                    Active = true,
                    Length = 10,
                    Width = 10,
                    Height = 10,
                    Weight = 10,
                    ShopId = shop.Id,
                }
            );
        }

        await dbContext.SaveChangesAsync();
        return dbContext;
    }
}
