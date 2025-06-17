using Ecommerce.Application.Categories.Commands;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Categories.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Commands;

public class DeleteCategoryTests
{
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task DeleteCategory_WithValidIdAndNoSubcategories_ShouldDeleteCategory()
    {
        // Arrange
        var category = new Category { Name = "Category To Delete" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteCategory.Command { Id = category.Id };
        var handler = new DeleteCategory.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Unit.Value);

        var deletedCategory = await _dbContext.Categories.FindAsync(category.Id);
        deletedCategory.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = 9999;
        var command = new DeleteCategory.Command { Id = invalidId };
        var handler = new DeleteCategory.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task DeleteCategory_WithSubcategories_ShouldReturnFailure()
    {
        // Arrange - Get a category with subcategories
        var categoryWithSubcategories = await _dbContext
            .Categories.Include(c => c.Subcategories)
            .Where(c => c.Subcategories.Any())
            .FirstAsync();

        var command = new DeleteCategory.Command { Id = categoryWithSubcategories.Id };
        var handler = new DeleteCategory.Handler(_dbContext);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(
            "Cannot delete category with subcategories. Remove subcategories first."
        );
        result.Code.ShouldBe(400);

        // Verify the category still exists
        var category = await _dbContext.Categories.FindAsync(categoryWithSubcategories.Id);
        category.ShouldNotBeNull();
    }
}
