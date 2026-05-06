using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Calculations;

public sealed record RunEmissionCalculationResponse(
    Guid TenantId,
    Guid CompanyId,
    Guid InventoryId,
    Guid CalculationRunId,
    string FactorSetCode,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal CarbonCreditsRequired,
    IReadOnlyCollection<ScopeEmissionSummary> ScopeSummaries);

public sealed record ScopeEmissionSummary(
    EmissionScope Scope,
    decimal TotalKgCo2e,
    decimal TotalTCo2e);
