using Ecommerce.Application.Categories.Commands;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Categories.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Commands;

public class DeleteSubcategoryTests
{
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task DeleteSubcategory_WithValidId_ShouldDeleteSubcategory()
    {
        // Arrange
        var category = await _dbContext.Categories.FirstAsync();
        var subcategory = new Subcategory
        {
            Name = "Subcategory To Delete",
            CategoryId = category.Id,
        };
        _dbContext.Subcategories.Add(subcategory);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteSubcategory.Command { Id = subcategory.Id };
        var handler = new DeleteSubcategory.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Unit.Value);

        var deletedSubcategory = await _dbContext.Subcategories.FindAsync(subcategory.Id);
        deletedSubcategory.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteSubcategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = 9999;
        var command = new DeleteSubcategory.Command { Id = invalidId };
        var handler = new DeleteSubcategory.Handler(_dbContext);

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
        // Arrange - Create a subcategory with associated products
        var category = await _dbContext.Categories.FirstAsync();
        var subcategory = new Subcategory
        {
            Name = "Subcategory With Products",
            CategoryId = category.Id,
        };

        _dbContext.Subcategories.Add(subcategory);
        await _dbContext.SaveChangesAsync();

        // Add a product and associate it with the subcategory
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            RegularPrice = 99.99m,
            Quantity = 10,
            Active = true,
            ShopId = "test-shop",
            Length = 10,
            Width = 10,
            Height = 10,
            Weight = 10,
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        // Associate product with subcategory
        _dbContext.ProductSubcategories.Add(
            new ProductSubcategory { ProductId = product.Id, SubcategoryId = subcategory.Id }
        );
        await _dbContext.SaveChangesAsync();

        var command = new DeleteSubcategory.Command { Id = subcategory.Id };
        var handler = new DeleteSubcategory.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cannot delete subcategory with associated products");
        result.Code.ShouldBe(400);

        // Verify the subcategory still exists
        var stillExists = await _dbContext.Subcategories.FindAsync(subcategory.Id);
        stillExists.ShouldNotBeNull();
    }
}
