using Amazon.S3.Model;
using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Products.Helpers;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Products.Commands;

public class EditProductTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task EditProduct_ShouldReturnUpdatedProduct()
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync();
        var command = new EditProduct.Command
        {
            Id = product!.Id,
            EditProductRequestDto = new EditProductRequestDto
            {
                Name = "Updated Product",
                Description = "Updated Product Description",
                RegularPrice = 200,
                Quantity = 20,
                Active = false,
                Length = 20,
                Width = 20,
                Height = 20,
                Weight = 20,
            },
        };
        var handler = new EditProduct.Handler(_dbContext, _mapper);

        var editResult = await handler.Handle(command, CancellationToken.None);

        editResult.Value.ShouldNotBeNull();
        editResult.Value.ShouldBeOfType<ProductResponseDto>();
        editResult.Value.Name.ShouldBe("Updated Product");
    }
}
