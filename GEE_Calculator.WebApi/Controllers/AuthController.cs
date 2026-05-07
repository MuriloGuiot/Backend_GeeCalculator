using GEE_Calculator.Domain.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthApplicationService authApplicationService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserSnapshot>> Me(CancellationToken cancellationToken)
    {
        return Ok(await authApplicationService.GetCurrentUserAsync(cancellationToken));
    }
}
