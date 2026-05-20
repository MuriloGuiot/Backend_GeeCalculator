using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Reports;

public sealed record EmissionsDashboardResponse(
    Guid TenantId,
    Guid? CompanyId,
    int? Year,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal TotalBiogenicKgCo2,
    decimal TotalBiogenicTCo2,
    decimal TotalBiogenicRemovalKgCo2,
    decimal TotalBiogenicRemovalTCo2,
    decimal CarbonCreditsRequired,
    IReadOnlyCollection<ScopeDashboardItem> Scopes,
    IReadOnlyCollection<CategoryDashboardItem> Categories,
    IReadOnlyCollection<MonthlyEmissionsItem> MonthlyTimeline);

public sealed record ScopeDashboardItem(
    EmissionScope Scope,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal BiogenicKgCo2,
    decimal BiogenicTCo2,
    decimal BiogenicRemovalKgCo2,
    decimal BiogenicRemovalTCo2);

public sealed record CategoryDashboardItem(
    EmissionScope Scope,
    string CategoryCode,
    string CategoryName,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal BiogenicKgCo2,
    decimal BiogenicTCo2,
    decimal BiogenicRemovalKgCo2,
    decimal BiogenicRemovalTCo2);

public sealed record MonthlyEmissionsItem(
    int Year,
    int? Month,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal BiogenicKgCo2,
    decimal BiogenicTCo2,
    decimal BiogenicRemovalKgCo2,
    decimal BiogenicRemovalTCo2);

public interface IReportService
{
    Task<EmissionsDashboardResponse> GetEmissionsDashboardAsync(
        Guid? companyId,
        int? year,
        CancellationToken cancellationToken = default);
}

public interface IReportRepository
{
    Task<IReadOnlyCollection<ScopeDashboardItem>> GetScopeTotalsAsync(Guid tenantId, Guid? companyId, int? year, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CategoryDashboardItem>> GetCategoryTotalsAsync(Guid tenantId, Guid? companyId, int? year, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MonthlyEmissionsItem>> GetMonthlyTimelineAsync(Guid tenantId, Guid? companyId, int? year, CancellationToken cancellationToken);
}
