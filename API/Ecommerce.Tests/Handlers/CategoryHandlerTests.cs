using AutoMapper;
using Ecommerce.Application.Categories.Commands;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Categories.Queries;
using Ecommerce.Application.Core;
using Ecommerce.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Handlers;

public class CategoryHandlerTests
{
    private IMapper _mapper = GetMapper();

    private static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
        return config.CreateMapper();
    }

    private static async Task<TestAppDbContext> GetDbContext()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TestAppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        // Seed test categories
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Clothing" },
            new Category { Id = 3, Name = "Books" },
        };
        await dbContext.Categories.AddRangeAsync(categories);

        // Seed test subcategories
        var subcategories = new List<Subcategory>
        {
            new Subcategory
            {
                Id = 1,
                Name = "Smartphones",
                CategoryId = 1,
            },
            new Subcategory
            {
                Id = 2,
                Name = "Laptops",
                CategoryId = 1,
            },
            new Subcategory
            {
                Id = 3,
                Name = "Men's Clothing",
                CategoryId = 2,
            },
            new Subcategory
            {
                Id = 4,
                Name = "Women's Clothing",
                CategoryId = 2,
            },
            new Subcategory
            {
                Id = 5,
                Name = "Fiction",
                CategoryId = 3,
            },
        };
        await dbContext.Subcategories.AddRangeAsync(subcategories);

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task ListCategories_ShouldReturnAllCategories()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListCategories.Query();
        var handler = new ListCategories.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(3);
        result.Value.ShouldContain(c => c.Name == "Electronics");
        result.Value.ShouldContain(c => c.Name == "Clothing");
        result.Value.ShouldContain(c => c.Name == "Books");
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCorrectCategory()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new GetCategoryById.Query { Id = 1 };
        var handler = new GetCategoryById.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Electronics");
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new GetCategoryById.Query { Id = 99 };
        var handler = new GetCategoryById.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task GetSubcategoryById_ShouldReturnCorrectSubcategory()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new GetSubcategoryById.Query { Id = 1 };
        var handler = new GetSubcategoryById.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Smartphones");
        result.Value.CategoryId.ShouldBe(1);
    }

    [Fact]
    public async Task GetSubcategoryById_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new GetSubcategoryById.Query { Id = 99 };
        var handler = new GetSubcategoryById.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Subcategory not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task CreateCategory_ShouldAddCategoryToDatabase()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new CreateCategory.Command
        {
            CreateCategoryRequestDto = new CreateCategoryRequestDto { Name = "Home & Kitchen" },
        };
        var handler = new CreateCategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Home & Kitchen");

        // Verify it's in the database
        var categoryInDb = await dbContext.Categories.FindAsync(result.Value.Id);
        categoryInDb.ShouldNotBeNull();
        categoryInDb.Name.ShouldBe("Home & Kitchen");
    }

    [Fact]
    public async Task EditCategory_ShouldUpdateExistingCategory()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new EditCategory.Command
        {
            Id = 1,
            EditCategoryRequestDto = new EditCategoryRequestDto { Name = "Updated Electronics" },
        };
        var handler = new EditCategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Updated Electronics");

        // Verify it's updated in the database
        var categoryInDb = await dbContext.Categories.FindAsync(1);
        categoryInDb.ShouldNotBeNull();
        categoryInDb.Name.ShouldBe("Updated Electronics");
    }

    [Fact]
    public async Task EditCategory_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new EditCategory.Command
        {
            Id = 99,
            EditCategoryRequestDto = new EditCategoryRequestDto { Name = "This Will Fail" },
        };
        var handler = new EditCategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
    }

    [Fact]
    public async Task DeleteCategory_WithNoSubcategories_ShouldRemoveCategory()
    {
        // Arrange
        var dbContext = await GetDbContext();

        // Add a category with no subcategories
        var newCategory = new Category { Id = 10, Name = "Test Category" };
        dbContext.Categories.Add(newCategory);
        await dbContext.SaveChangesAsync();

        var command = new DeleteCategory.Command { Id = 10 };
        var handler = new DeleteCategory.Handler(dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify it's removed from the database
        var categoryInDb = await dbContext.Categories.FindAsync(10);
        categoryInDb.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteCategory_WithSubcategories_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new DeleteCategory.Command { Id = 1 }; // Electronics has subcategories
        var handler = new DeleteCategory.Handler(dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Cannot delete category with subcategories");

        // Verify the category still exists
        var categoryInDb = await dbContext.Categories.FindAsync(1);
        categoryInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new DeleteCategory.Command { Id = 99 };
        var handler = new DeleteCategory.Handler(dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task CreateSubcategory_ShouldAddSubcategoryToDatabase()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new CreateSubcategory.Command
        {
            CreateSubcategoryRequestDto = new CreateSubcategoryRequestDto
            {
                Name = "Tablets",
                CategoryId = 1,
            },
        };
        var handler = new CreateSubcategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Tablets");
        result.Value.CategoryId.ShouldBe(1);

        // Verify it's in the database
        var subcategories = await dbContext
            .Subcategories.Where(s => s.Name == "Tablets")
            .ToListAsync();
        subcategories.Count.ShouldBe(1);
        subcategories[0].CategoryId.ShouldBe(1);
    }

    [Fact]
    public async Task CreateSubcategory_WithInvalidCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new CreateSubcategory.Command
        {
            CreateSubcategoryRequestDto = new CreateSubcategoryRequestDto
            {
                Name = "Invalid",
                CategoryId = 99,
            },
        };
        var handler = new CreateSubcategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
    }

    [Fact]
    public async Task EditSubcategory_ShouldUpdateExistingSubcategory()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new EditSubcategory.Command
        {
            Id = 1,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = "Updated Smartphones",
                CategoryId = 1,
            },
        };
        var handler = new EditSubcategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Updated Smartphones");
        result.Value.CategoryId.ShouldBe(1);

        // Verify it's updated in the database
        var subcategoryInDb = await dbContext.Subcategories.FindAsync(1);
        subcategoryInDb.ShouldNotBeNull();
        subcategoryInDb.Name.ShouldBe("Updated Smartphones");
    }

    [Fact]
    public async Task EditSubcategory_WithChangedCategory_ShouldUpdateCategoYRelation()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new EditSubcategory.Command
        {
            Id = 1,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = "Smartphones",
                CategoryId = 2, // Move from Electronics to Clothing
            },
        };
        var handler = new EditSubcategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CategoryId.ShouldBe(2);

        // Verify it's updated in the database
        var subcategoryInDb = await dbContext.Subcategories.FindAsync(1);
        subcategoryInDb.ShouldNotBeNull();
        subcategoryInDb.CategoryId.ShouldBe(2);
    }

    [Fact]
    public async Task EditSubcategory_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new EditSubcategory.Command
        {
            Id = 99,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = "This Will Fail",
                CategoryId = 1,
            },
        };
        var handler = new EditSubcategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Subcategory not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task EditSubcategory_WithInvalidCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new EditSubcategory.Command
        {
            Id = 1,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = "Smartphones",
                CategoryId = 99,
            },
        };
        var handler = new EditSubcategory.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Target category not found");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task DeleteSubcategory_WithNoProducts_ShouldRemoveSubcategory()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new DeleteSubcategory.Command { Id = 1 };
        var handler = new DeleteSubcategory.Handler(dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify it's removed from the database
        var subcategoryInDb = await dbContext.Subcategories.FindAsync(1);
        subcategoryInDb.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteSubcategory_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var command = new DeleteSubcategory.Command { Id = 99 };
        var handler = new DeleteSubcategory.Handler(dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Subcategory not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task DeleteSubcategory_WithAssociatedProducts_ShouldReturnFailure()
    {
        // Arrange
        var dbContext = await GetDbContext();

        // Add a product associated with subcategory
        var product = new Product
        {
            Id = 1,
            Name = "iPhone 15",
            RegularPrice = 999.99M,
            Quantity = 10,
            ShopId = "test-shop",
            Active = true,
            Description = "Latest iPhone model",
            Length = 1,
            Width = 1,
            Height = 1,
            Weight = 1,
        };
        await dbContext.Products.AddAsync(product);

        // Associate product with subcategory
        var subcategory = await dbContext.Subcategories.FindAsync(1);
        product.Subcategories.Add(subcategory!);
        await dbContext.SaveChangesAsync();

        var command = new DeleteSubcategory.Command { Id = 1 };
        var handler = new DeleteSubcategory.Handler(dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Cannot delete subcategory with associated products");

        // Verify the subcategory still exists
        var subcategoryInDb = await dbContext.Subcategories.FindAsync(1);
        subcategoryInDb.ShouldNotBeNull();
    }
}
