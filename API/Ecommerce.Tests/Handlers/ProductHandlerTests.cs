using Amazon.S3.Model;
using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Handlers;

public class ProductHandlerTests
{
    private IMapper _mapper = GetMapper();
    private IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private IPhotoService _photoService = A.Fake<IPhotoService>();

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
        if (await dbContext.Products.AnyAsync())
            return dbContext;
        var shop = new User
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = "Shop Test",
            Address = "Shop Address",
            WardId = 1,
        };

        dbContext.Users.Add(shop);

        for (var i = 0; i < 10; i++)
        {
            dbContext.Products.Add(
                new Product
                {
                    Name = $"Product {i}",
                    Description = $"Description {i}",
                    RegularPrice = 110 + i,
                    DiscountPrice = 105 + i,
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

    [Fact]
    public async Task ListProducts_ShouldReturnPagedListOfProducts()
    {
        var dbContext = await GetDbContext();
        var query = new ListProducts.Query();
        var handler = new ListProducts.Handler(dbContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<PagedList<ProductResponseDto>>();
    }

    [Fact]
    public async Task ListProducts_ShouldReturnFilteredProducts()
    {
        var dbContext = await GetDbContext();
        var query = new ListProducts.Query
        {
            MinPrice = 100,
            MaxPrice = 110,
            PageSize = 5,
            PageNumber = 1,
        };
        var handler = new ListProducts.Handler(dbContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<PagedList<ProductResponseDto>>();
        result.Value.Items.Count.ShouldBeLessThanOrEqualTo(5);
        result.Value.Items.ShouldAllBe(i =>
            (i.DiscountPrice ?? i.RegularPrice) >= 100 && (i.DiscountPrice ?? i.RegularPrice) <= 110
        );
    }

    [Fact]
    public async Task GetProductByIds_ShouldReturnProducts()
    {
        var dbContext = await GetDbContext();
        var query = new GetProductById.Query { Id = 1 };
        var handler = new GetProductById.Handler(dbContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result.Value);
        Assert.IsType<ProductResponseDto>(result.Value);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedProduct()
    {
        var dbContext = await GetDbContext();
        A.CallTo(() => _userAccessor.GetUserAsync())
            .Returns(
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayName = "Test User",
                    Address = "Test Address",
                    WardId = 1,
                }
            );
        var query = new CreateProduct.Command
        {
            CreateProductRequestDto = new CreateProductRequestDto
            {
                Name = "New Product",
                Description = "New Product Description",
                RegularPrice = 100,
                DiscountPrice = 90,
                Quantity = 10,
                Active = true,
                Length = 10,
                Width = 10,
                Height = 10,
                Weight = 10,
            },
        };
        var handler = new CreateProduct.Handler(dbContext, _mapper, _userAccessor);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ProductResponseDto>();
        result.Value.Name.ShouldBe("New Product");
        result.Value.ShopId.ShouldBe(_userAccessor.GetUserAsync().Result.Id);
        result.Value.ShopName.ShouldBe(_userAccessor.GetUserAsync().Result.DisplayName);
    }

    [Fact]
    public async Task EditProduct_ShouldReturnUpdatedProduct()
    {
        var dbContext = await GetDbContext();
        var product = await dbContext.Products.FirstOrDefaultAsync();
        var command = new EditProduct.Command
        {
            Id = product!.Id,
            EditProductRequestDto = new EditProductRequestDto
            {
                Name = "Updated Product",
                Description = "Updated Product Description",
                RegularPrice = 200,
                DiscountPrice = 180,
                Quantity = 20,
                Active = false,
                Length = 20,
                Width = 20,
                Height = 20,
                Weight = 20,
            },
        };
        var handler = new EditProduct.Handler(dbContext, _mapper);

        var editResult = await handler.Handle(command, CancellationToken.None);

        editResult.Value.ShouldNotBeNull();
        editResult.Value.ShouldBeOfType<ProductResponseDto>();
        editResult.Value.Name.ShouldBe("Updated Product");
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnSuccess()
    {
        var dbContext = await GetDbContext();
        var product = await dbContext.Products.FirstOrDefaultAsync();
        var command = new DeleteProduct.Command { Id = product!.Id };
        var handler = new DeleteProduct.Handler(dbContext);

        var deleteResult = await handler.Handle(command, CancellationToken.None);
        var deletedProduct = await dbContext.Products.FindAsync(product.Id);

        deleteResult.IsSuccess.ShouldBeTrue();
        deleteResult.Error.ShouldBeNull();
        deletedProduct.ShouldBeNull();
    }

    [Fact]
    public async Task AddProductPhoto_ShouldReturnSuccess()
    {
        var dbContext = await GetDbContext();
        var product = await dbContext
            .Products.Include(product => product.Photos)
            .FirstOrDefaultAsync();
        var file = A.Fake<IFormFile>();
        A.CallTo(() => _photoService.UploadPhoto(file, A<string>._))
            .Returns(
                new S3UploadResult
                {
                    Key = $"photos/products/{product!.Id}/{Guid.NewGuid().ToString()}-file.jpg",
                    PutObjectResponse = new PutObjectResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK,
                    },
                }
            );

        var command = new AddProductPhoto.Command { ProductId = product.Id, File = file };
        var handler = new AddProductPhoto.Handler(dbContext, _photoService);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();
        product.Photos.ShouldContain(result.Value);
    }

    [Fact]
    public async Task UpdateProductPhotoDisplayOrder_ShouldReturnSuccess()
    {
        var dbContext = await GetDbContext();
        var product = await dbContext
            .Products.Include(product => product.Photos)
            .FirstOrDefaultAsync();
        // add photos to product
        var photos = new List<ProductPhoto>
        {
            new ProductPhoto
            {
                Key = "photos/products/1/photo1.jpg",
                DisplayOrder = 1,
                ProductId = product!.Id,
            },
            new ProductPhoto
            {
                Key = "photos/products/1/photo2.jpg",
                DisplayOrder = 2,
                ProductId = product.Id,
            },
            new ProductPhoto
            {
                Key = "photos/products/1/photo3.jpg",
                DisplayOrder = 3,
                ProductId = product.Id,
            },
        };
        product.Photos.Clear();
        dbContext.ProductPhotos.AddRange(photos);
        await dbContext.SaveChangesAsync();

        var command = new UpdateProductPhotoDisplayOrder.Command
        {
            ProductId = product.Id,
            PhotoOrders = new List<UpdateProductPhotoDisplayOrderRequestDto>
            {
                new UpdateProductPhotoDisplayOrderRequestDto
                {
                    Key = "photos/products/1/photo2.jpg",
                    DisplayOrder = 1,
                },
                new UpdateProductPhotoDisplayOrderRequestDto
                {
                    Key = "photos/products/1/photo3.jpg",
                    DisplayOrder = 2,
                },
                new UpdateProductPhotoDisplayOrderRequestDto
                {
                    Key = "photos/products/1/photo1.jpg",
                    DisplayOrder = 3,
                },
            },
        };
        var handler = new UpdateProductPhotoDisplayOrder.Handler(dbContext);

        var result = await handler.Handle(command, CancellationToken.None);
        var updatedPhotos = await dbContext
            .ProductPhotos.Where(p => p.ProductId == product.Id)
            .ToListAsync();

        result.IsSuccess.ShouldBeTrue();
        updatedPhotos.ShouldNotBeEmpty();
        updatedPhotos.ShouldContain(p =>
            p.Key == "photos/products/1/photo2.jpg" && p.DisplayOrder == 1
        );
        updatedPhotos.ShouldContain(p =>
            p.Key == "photos/products/1/photo3.jpg" && p.DisplayOrder == 2
        );
        updatedPhotos.ShouldContain(p =>
            p.Key == "photos/products/1/photo1.jpg" && p.DisplayOrder == 3
        );
    }

    [Fact]
    public async Task DeleteProductPhoto_ShouldReturnSuccess()
    {
        var dbContext = await GetDbContext();
        var product = await dbContext
            .Products.Include(product => product.Photos)
            .FirstOrDefaultAsync();
        var photo = new ProductPhoto
        {
            Key = "photos/products/1/photo1.jpg",
            DisplayOrder = 1,
            ProductId = product!.Id,
        };
        product.Photos.Clear();
        product.Photos.Add(photo);
        dbContext.ProductPhotos.Add(photo);
        await dbContext.SaveChangesAsync();

        var command = new DeleteProductPhoto.Command { Key = photo.Key, ProductId = product.Id };
        var handler = new DeleteProductPhoto.Handler(dbContext, _photoService);

        var result = await handler.Handle(command, CancellationToken.None);
        var deletedPhoto = await dbContext.ProductPhotos.FindAsync(photo.Key);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        deletedPhoto.ShouldBeNull();
    }
}
