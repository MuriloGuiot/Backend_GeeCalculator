using GEE_Calculator.Domain.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ICurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<CurrentUserSnapshot> Me()
    {
        return Ok(currentUserContext.GetCurrentUser());
    }
}
