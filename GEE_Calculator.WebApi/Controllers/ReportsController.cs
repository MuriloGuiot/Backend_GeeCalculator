using GEE_Calculator.Domain.Reports;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("emissions-dashboard")]
    public async Task<ActionResult<EmissionsDashboardResponse>> GetEmissionsDashboard(
        [FromQuery] Guid? companyId = null,
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await reportService.GetEmissionsDashboardAsync(companyId, year, cancellationToken));
    }
}
