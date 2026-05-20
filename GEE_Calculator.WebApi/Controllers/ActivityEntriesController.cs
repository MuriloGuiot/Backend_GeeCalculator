using GEE_Calculator.Domain.ActivityEntries;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/inventories/{inventoryId:guid}/entries")]
public sealed class ActivityEntriesController(IActivityEntryService activityEntryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ActivityEntryResponse>>> List(
        Guid inventoryId,
        CancellationToken cancellationToken)
    {
        return Ok(await activityEntryService.ListAsync(inventoryId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ActivityEntryResponse>> Create(
        Guid inventoryId,
        CreateActivityEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await activityEntryService.CreateAsync(inventoryId, request, cancellationToken);
            return CreatedAtAction(nameof(List), new { inventoryId }, entry);
        }
        catch (ActivityEntryException exception)
        {
            return BadRequest(new
            {
                error = "activity_entry_error",
                message = exception.Message
            });
        }
    }

    [HttpPut("{entryId:guid}")]
    public async Task<ActionResult<ActivityEntryResponse>> Update(
        Guid inventoryId,
        Guid entryId,
        UpdateActivityEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await activityEntryService.UpdateAsync(inventoryId, entryId, request, cancellationToken);
            return entry is null ? NotFound() : Ok(entry);
        }
        catch (ActivityEntryException exception)
        {
            return BadRequest(new
            {
                error = "activity_entry_error",
                message = exception.Message
            });
        }
    }

    [HttpDelete("{entryId:guid}")]
    public async Task<IActionResult> Delete(
        Guid inventoryId,
        Guid entryId,
        CancellationToken cancellationToken)
    {
        var deleted = await activityEntryService.DeleteAsync(inventoryId, entryId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
