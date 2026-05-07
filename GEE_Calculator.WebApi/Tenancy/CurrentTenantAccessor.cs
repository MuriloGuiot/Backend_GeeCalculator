using System.Security.Claims;
using GEE_Calculator.Domain.Tenancy;
using Microsoft.Extensions.Options;

namespace GEE_Calculator.WebApi.Tenancy;

public sealed class CurrentTenantAccessor(
    IHttpContextAccessor httpContextAccessor,
    IOptions<TenancyOptions> options) : ICurrentTenantAccessor
{
    public CurrentTenantSnapshot GetCurrentTenant()
    {
        var context = httpContextAccessor.HttpContext;
        var headers = context?.Request.Headers;
        var user = context?.User;
        var allowHeaderFallback = options.Value.AllowTenantHeaderFallback;
        var tenantId = ReadClaim(user, "tenant_id", "tenantId", "organization_id", "organizationId")
            ?? ReadHeaderWhenAllowed(headers, TenantRequestHeaders.TenantId, allowHeaderFallback);
        var companyId = ReadClaim(user, "company_id", "companyId")
            ?? ReadHeaderWhenAllowed(headers, TenantRequestHeaders.CompanyId, allowHeaderFallback);
        var apiKey = ReadHeader(headers, TenantRequestHeaders.ApiKey);

        return new CurrentTenantSnapshot(
            TenantId: tenantId,
            CompanyId: companyId,
            ApiKeyPrefix: apiKey is null ? null : apiKey[..Math.Min(8, apiKey.Length)],
            IsResolved: !string.IsNullOrWhiteSpace(tenantId));
    }

    private static string? ReadHeaderWhenAllowed(IHeaderDictionary? headers, string headerName, bool allowHeaderFallback)
    {
        return allowHeaderFallback ? ReadHeader(headers, headerName) : null;
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
