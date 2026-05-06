using GEE_Calculator.Domain.Calculations;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/calculations")]
public sealed class CalculationsController(IEmissionCalculationService calculationService) : ControllerBase
{
    [HttpPost("preview")]
    public ActionResult<CalculateEmissionPreviewResponse> Preview(CalculateEmissionPreviewRequest request)
    {
        try
        {
            return Ok(calculationService.Preview(request));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new
            {
                error = exception.ParamName,
                message = exception.Message
            });
        }
    }

    [HttpPost("run")]
    public async Task<ActionResult<RunEmissionCalculationResponse>> Run(
        RunEmissionCalculationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await calculationService.RunAsync(request, cancellationToken));
        }
        catch (EmissionCalculationException exception)
        {
            return BadRequest(new
            {
                error = "calculation_error",
                message = exception.Message
            });
        }
    }
}
