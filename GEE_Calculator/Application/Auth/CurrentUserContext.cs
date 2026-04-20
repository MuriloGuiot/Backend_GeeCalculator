using System.Security.Claims;

namespace GEE_Calculator.Application.Auth;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public CurrentUserSnapshot GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated is not true)
        {
            return new CurrentUserSnapshot(
                Subject: null,
                Email: null,
                Name: null,
                TenantId: ReadHeader("X-Tenant-Id"),
                CompanyId: ReadHeader("X-Company-Id"),
                Roles: [],
                IsAuthenticated: false);
        }

        return new CurrentUserSnapshot(
            Subject: ReadClaim(user, ClaimTypes.NameIdentifier, "sub"),
            Email: ReadClaim(user, ClaimTypes.Email, "email"),
            Name: ReadClaim(user, ClaimTypes.Name, "name", "preferred_username"),
            TenantId: ReadClaim(user, "tenant_id", "tenantId") ?? ReadHeader("X-Tenant-Id"),
            CompanyId: ReadClaim(user, "company_id", "companyId") ?? ReadHeader("X-Company-Id"),
            Roles: ReadRoles(user),
            IsAuthenticated: true);
    }

    private string? ReadHeader(string name)
    {
        var headers = httpContextAccessor.HttpContext?.Request.Headers;
        return headers is not null && headers.TryGetValue(name, out var value) ? value.ToString() : null;
    }

    private static string? ReadClaim(ClaimsPrincipal user, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = user.FindFirstValue(claimType);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static IReadOnlyCollection<string> ReadRoles(ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role)
            .Concat(user.FindAll("role"))
            .Concat(user.FindAll("roles"))
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
