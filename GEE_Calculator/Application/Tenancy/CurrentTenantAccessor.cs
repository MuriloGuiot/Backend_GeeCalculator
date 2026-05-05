using System.Security.Claims;

namespace GEE_Calculator.Application.Tenancy;

public sealed class CurrentTenantAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentTenantAccessor
{
    public CurrentTenantSnapshot GetCurrentTenant()
    {
        var context = httpContextAccessor.HttpContext;
        var headers = context?.Request.Headers;
        var user = context?.User;
        var tenantId = ReadHeader(headers, TenantRequestHeaders.TenantId) ?? ReadClaim(user, "tenant_id", "tenantId");
        var companyId = ReadHeader(headers, TenantRequestHeaders.CompanyId) ?? ReadClaim(user, "company_id", "companyId");
        var apiKey = ReadHeader(headers, TenantRequestHeaders.ApiKey);

        return new CurrentTenantSnapshot(
            TenantId: tenantId,
            CompanyId: companyId,
            ApiKeyPrefix: apiKey is null ? null : apiKey[..Math.Min(8, apiKey.Length)],
            IsResolved: !string.IsNullOrWhiteSpace(tenantId));
    }

    private static string? ReadHeader(IHeaderDictionary? headers, string headerName)
    {
        return headers is not null && headers.TryGetValue(headerName, out var value)
            ? value.ToString()
            : null;
    }

    private static string? ReadClaim(ClaimsPrincipal? user, params string[] claimTypes)
    {
        if (user?.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

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
}
