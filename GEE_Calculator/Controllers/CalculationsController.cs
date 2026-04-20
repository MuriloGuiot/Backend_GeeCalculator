using GEE_Calculator.Application.Calculations;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.Controllers;

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
}
