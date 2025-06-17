using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests.Application.Categories.Helpers;

public static class CategoryTestHelper
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
        if (await dbContext.Categories.AnyAsync())
            return dbContext;

        // Add sample categories
        var categories = new List<Category>
        {
            new Category { Name = "Electronics" },
            new Category { Name = "Clothing" },
            new Category { Name = "Books" },
        };

        // Add sample subcategories
        var electronicsSubcategories = new List<Subcategory>
        {
            new Subcategory { Name = "Smartphones", Category = categories[0] },
            new Subcategory { Name = "Laptops", Category = categories[0] },
            new Subcategory { Name = "Tablets", Category = categories[0] },
        };

        var clothingSubcategories = new List<Subcategory>
        {
            new Subcategory { Name = "Men", Category = categories[1] },
            new Subcategory { Name = "Women", Category = categories[1] },
        };

        dbContext.Categories.AddRange(categories);
        dbContext.Subcategories.AddRange(electronicsSubcategories);
        dbContext.Subcategories.AddRange(clothingSubcategories);

        await dbContext.SaveChangesAsync();
        return dbContext;
    }
}
