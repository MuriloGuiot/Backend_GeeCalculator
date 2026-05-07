using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Inventories;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/inventories")]
public sealed class InventoriesController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<InventorySummaryResponse>>> List(
        [FromQuery] Guid? companyId = null,
        [FromQuery] int? year = null,
        [FromQuery] PeriodType? periodType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await inventoryService.ListAsync(companyId, year, periodType, page, pageSize, cancellationToken));
    }

    [HttpGet("{inventoryId:guid}")]
    public async Task<ActionResult<InventoryDetailsResponse>> Get(Guid inventoryId, CancellationToken cancellationToken)
    {
        var inventory = await inventoryService.GetAsync(inventoryId, cancellationToken);
        return inventory is null ? NotFound() : Ok(inventory);
    }
}
