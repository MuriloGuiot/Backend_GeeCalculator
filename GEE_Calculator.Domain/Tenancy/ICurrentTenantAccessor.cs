namespace GEE_Calculator.Domain.Tenancy;

public interface ICurrentTenantAccessor
{
    CurrentTenantSnapshot GetCurrentTenant();
}
