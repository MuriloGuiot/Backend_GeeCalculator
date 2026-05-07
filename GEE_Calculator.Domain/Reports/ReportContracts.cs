using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Reports;

public sealed record EmissionsDashboardResponse(
    Guid TenantId,
    Guid? CompanyId,
    int? Year,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal CarbonCreditsRequired,
    IReadOnlyCollection<ScopeDashboardItem> Scopes,
    IReadOnlyCollection<MonthlyEmissionsItem> MonthlyTimeline);

public sealed record ScopeDashboardItem(
    EmissionScope Scope,
    decimal TotalKgCo2e,
    decimal TotalTCo2e);

public sealed record MonthlyEmissionsItem(
    int Year,
    int? Month,
    decimal TotalKgCo2e,
    decimal TotalTCo2e);

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
    Task<IReadOnlyCollection<MonthlyEmissionsItem>> GetMonthlyTimelineAsync(Guid tenantId, Guid? companyId, int? year, CancellationToken cancellationToken);
}
