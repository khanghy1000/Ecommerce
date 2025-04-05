using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Application.Locations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class LocationsController : BaseApiController
{
    [HttpGet("provinces")]
    public async Task<ActionResult<List<ProvinceResponseDto>>> GetProvinces()
    {
        var result = await Mediator.Send(new ListProvinces.Query());
        return HandleResult(result);
    }

    [HttpGet("districts")]
    public async Task<ActionResult<List<DistrictResponseDto>>> GetDistricts(
        [FromQuery] int? provinceId
    )
    {
        var result = await Mediator.Send(new ListDistrict.Query { ProvinceId = provinceId });
        return HandleResult(result);
    }

    [HttpGet("wards")]
    public async Task<ActionResult<List<WardResponseDto>>> GetWards([FromQuery] int? districtId)
    {
        var result = await Mediator.Send(new ListWards.Query { DistrictId = districtId });
        return HandleResult(result);
    }
}
