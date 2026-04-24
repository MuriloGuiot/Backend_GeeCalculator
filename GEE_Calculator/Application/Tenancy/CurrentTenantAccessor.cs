namespace GEE_Calculator.Application.Tenancy;

public sealed class CurrentTenantAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentTenantAccessor
{
    public CurrentTenantSnapshot GetCurrentTenant()
    {
        var headers = httpContextAccessor.HttpContext?.Request.Headers;
        var tenantId = ReadHeader(headers, TenantRequestHeaders.TenantId);
        var companyId = ReadHeader(headers, TenantRequestHeaders.CompanyId);
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
}
