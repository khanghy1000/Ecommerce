using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecommerce.Application.Core;
using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Domain;
using Ecommerce.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Locations.Queries;

public class ListWards
{
    public class Query : IRequest<Result<List<WardResponseDto>>>
    {
        public int? DistrictId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IMapper mapper)
        : IRequestHandler<Query, Result<List<WardResponseDto>>>
    {
        public async Task<Result<List<WardResponseDto>>> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var query = dbContext.Wards.AsQueryable();

            if (request.DistrictId.HasValue)
            {
                query = query.Where(x => x.DistrictId == request.DistrictId.Value);
            }

            var wards = await query
                .OrderBy(x => x.Name)
                .ProjectTo<WardResponseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<WardResponseDto>>.Success(wards);
        }
    }
}
