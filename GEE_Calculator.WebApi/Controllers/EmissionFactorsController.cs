using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.EmissionFactors;
using GEE_Calculator.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/emission-factors")]
public sealed class EmissionFactorsController(IEmissionFactorCatalogService catalogService) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyCollection<EmissionCategoryResponse>>> ListCategories(
        [FromQuery] EmissionScope? scope = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await catalogService.ListCategoriesAsync(scope, cancellationToken));
    }

    [HttpGet("activity-units")]
    public async Task<ActionResult<IReadOnlyCollection<ActivityUnitResponse>>> ListActivityUnits(
        CancellationToken cancellationToken)
    {
        return Ok(await catalogService.ListActivityUnitsAsync(cancellationToken));
    }

    [HttpGet("factor-sets")]
    public async Task<ActionResult<IReadOnlyCollection<EmissionFactorSetResponse>>> ListFactorSets(
        CancellationToken cancellationToken)
    {
        return Ok(await catalogService.ListFactorSetsAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<EmissionFactorResponse>>> ListFactors(
        [FromQuery] string? factorSetCode = null,
        [FromQuery] EmissionScope? scope = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await catalogService.ListFactorsAsync(factorSetCode, scope, page, pageSize, cancellationToken));
    }
}
