using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Application.Tenancy;

internal static class TenantContextResolver
{
    public static Guid ResolveRequiredTenantId(ICurrentTenantAccessor currentTenantAccessor)
    {
        var tenantIdHeader = currentTenantAccessor.GetCurrentTenant().TenantId;

        if (!Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            throw new TenantContextException("The X-Tenant-Id header must be a valid GUID.");
        }

        return tenantId;
    }
}
