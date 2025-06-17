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

public class CreateProductTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedProduct()
    {
        A.CallTo(() => _userAccessor.GetUserAsync())
            .Returns(new User { Id = Guid.NewGuid().ToString(), DisplayName = "Test User" });
        var query = new CreateProduct.Command
        {
            CreateProductRequestDto = new CreateProductRequestDto
            {
                Name = "New Product",
                Description = "New Product Description",
                RegularPrice = 100,
                Quantity = 10,
                Active = true,
                Length = 10,
                Width = 10,
                Height = 10,
                Weight = 10,
            },
        };
        var handler = new CreateProduct.Handler(_dbContext, _mapper, _userAccessor);

        var result = await handler.Handle(query, CancellationToken.None);
        var shop = await _userAccessor.GetUserAsync();

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ProductResponseDto>();
        result.Value.Name.ShouldBe("New Product");
        result.Value.ShopId.ShouldBe(shop.Id);
        result.Value.ShopName.ShouldBe(shop.DisplayName);
    }
}
