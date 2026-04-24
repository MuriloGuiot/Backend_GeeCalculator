namespace GEE_Calculator.Application.Tenancy;

public sealed record CurrentTenantSnapshot(
    string? TenantId,
    string? CompanyId,
    string? ApiKeyPrefix,
    bool IsResolved)
{
    public bool HasTenant => !string.IsNullOrWhiteSpace(TenantId);
}
