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
    decimal TotalBiogenicKgCo2,
    decimal TotalBiogenicTCo2,
    decimal TotalBiogenicRemovalKgCo2,
    decimal TotalBiogenicRemovalTCo2,
    decimal CarbonCreditsRequired,
    IReadOnlyCollection<ScopeEmissionSummary> ScopeSummaries,
    IReadOnlyCollection<CategoryEmissionSummary> CategorySummaries);

public sealed record ScopeEmissionSummary(
    EmissionScope Scope,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal BiogenicKgCo2,
    decimal BiogenicTCo2,
    decimal BiogenicRemovalKgCo2,
    decimal BiogenicRemovalTCo2);

public sealed record CategoryEmissionSummary(
    EmissionScope Scope,
    string CategoryCode,
    string CategoryName,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal BiogenicKgCo2,
    decimal BiogenicTCo2,
    decimal BiogenicRemovalKgCo2,
    decimal BiogenicRemovalTCo2);
