using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Locations.Queries;
using Ecommerce.Domain;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Ecommerce.Tests.Handlers;

public class LocationHandlerTests
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

        // Seed test provinces
        var provinces = new List<Province>
        {
            new Province { Id = 1, Name = "Hanoi" },
            new Province { Id = 2, Name = "Ho Chi Minh" },
            new Province { Id = 3, Name = "Da Nang" },
        };
        await dbContext.Provinces.AddRangeAsync(provinces);

        // Seed test districts
        var districts = new List<District>
        {
            new District
            {
                Id = 1,
                Name = "Ba Dinh",
                ProvinceId = 1,
            },
            new District
            {
                Id = 2,
                Name = "Hoan Kiem",
                ProvinceId = 1,
            },
            new District
            {
                Id = 3,
                Name = "District 1",
                ProvinceId = 2,
            },
            new District
            {
                Id = 4,
                Name = "District 2",
                ProvinceId = 2,
            },
            new District
            {
                Id = 5,
                Name = "Hai Chau",
                ProvinceId = 3,
            },
        };
        await dbContext.Districts.AddRangeAsync(districts);

        // Seed test wards
        var wards = new List<Ward>
        {
            new Ward
            {
                Id = 1,
                Name = "Phuc Xa",
                DistrictId = 1,
            },
            new Ward
            {
                Id = 2,
                Name = "Truc Bach",
                DistrictId = 1,
            },
            new Ward
            {
                Id = 3,
                Name = "Hang Bac",
                DistrictId = 2,
            },
            new Ward
            {
                Id = 4,
                Name = "Hang Dao",
                DistrictId = 2,
            },
            new Ward
            {
                Id = 5,
                Name = "Ben Nghe",
                DistrictId = 3,
            },
            new Ward
            {
                Id = 6,
                Name = "Da Kao",
                DistrictId = 4,
            },
            new Ward
            {
                Id = 7,
                Name = "Hai Chau 1",
                DistrictId = 5,
            },
        };
        await dbContext.Wards.AddRangeAsync(wards);

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task ListProvinces_ShouldReturnAllProvincesOrderedByName()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListProvinces.Query();
        var handler = new ListProvinces.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(3);

        // Check if ordered by name
        result.Value[0].Name.ShouldBe("Da Nang");
        result.Value[1].Name.ShouldBe("Hanoi");
        result.Value[2].Name.ShouldBe("Ho Chi Minh");
    }

    [Fact]
    public async Task ListDistrict_ShouldReturnAllDistrictsOrderedByName()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListDistrict.Query();
        var handler = new ListDistrict.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(5);

        // Verify districts are ordered by name
        var districts = result.Value.OrderBy(d => d.Name).ToList();
        districts[0].Name.ShouldBe("Ba Dinh");
        districts[1].Name.ShouldBe("District 1");
        districts[2].Name.ShouldBe("District 2");
    }

    [Fact]
    public async Task ListDistrict_WithProvinceId_ShouldReturnFilteredDistricts()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListDistrict.Query { ProvinceId = 1 };
        var handler = new ListDistrict.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldAllBe(d => d.ProvinceId == 1);
        result.Value.ShouldContain(d => d.Name == "Ba Dinh");
        result.Value.ShouldContain(d => d.Name == "Hoan Kiem");
    }

    [Fact]
    public async Task ListDistrict_WithNonExistentProvinceId_ShouldReturnEmptyList()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListDistrict.Query { ProvinceId = 99 };
        var handler = new ListDistrict.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ListWards_ShouldReturnAllWardsOrderedByName()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListWards.Query();
        var handler = new ListWards.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(7);

        // Verify wards are ordered by name
        var wards = result.Value.OrderBy(w => w.Name).ToList();
        wards[0].Name.ShouldBe("Ben Nghe");
        wards[1].Name.ShouldBe("Da Kao");
    }

    [Fact]
    public async Task ListWards_WithDistrictId_ShouldReturnFilteredWards()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListWards.Query { DistrictId = 1 };
        var handler = new ListWards.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldAllBe(w => w.DistrictId == 1);
        result.Value.ShouldContain(w => w.Name == "Phuc Xa");
        result.Value.ShouldContain(w => w.Name == "Truc Bach");
    }

    [Fact]
    public async Task ListWards_WithNonExistentDistrictId_ShouldReturnEmptyList()
    {
        // Arrange
        var dbContext = await GetDbContext();
        var query = new ListWards.Query { DistrictId = 99 };
        var handler = new ListWards.Handler(dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(0);
    }
}
