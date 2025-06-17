using AutoMapper;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Categories.Queries;
using Ecommerce.Tests.Application.Categories.Helpers;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Queries;

public class ListCategoriesTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task ListCategories_ShouldReturnAllCategoriesWithSubcategories()
    {
        // Arrange
        var query = new ListCategories.Query();
        var handler = new ListCategories.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<List<CategoryResponseDto>>();
        result.Value.Count.ShouldBeGreaterThan(0);

        var categoryWithSubcategories = result.Value.FirstOrDefault(c => c.Subcategories.Count > 0);
        categoryWithSubcategories.ShouldNotBeNull();
        categoryWithSubcategories.Subcategories.ShouldNotBeEmpty();
    }
}
