namespace GEE_Calculator.Application.Tenancy;

public interface ICurrentTenantAccessor
{
    CurrentTenantSnapshot GetCurrentTenant();
}
