using GEE_Calculator.Application.Auth;

namespace GEE_Calculator.WebApi.Auth;

public sealed class HttpAccessTokenReader(IHttpContextAccessor httpContextAccessor) : IAccessTokenReader
{
    public string? GetBearerToken()
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorization["Bearer ".Length..].Trim();
    }
}
