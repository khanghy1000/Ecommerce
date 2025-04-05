using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Locations.Queries;

public class ListProvinces
{
    public class Query : IRequest<Result<List<ProvinceResponseDto>>> { }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<ProvinceResponseDto>>>
    {
        public async Task<Result<List<ProvinceResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var provinces = await dbContext
                .Provinces.OrderBy(x => x.Name)
                .ProjectTo<ProvinceResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<ProvinceResponseDto>>.Success(provinces);
        }
    }
}
