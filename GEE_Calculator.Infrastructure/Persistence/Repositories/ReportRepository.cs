using GEE_Calculator.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository(GeeCalculatorDbContext dbContext) : IReportRepository
{
    public async Task<IReadOnlyCollection<ScopeDashboardItem>> GetScopeTotalsAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        CancellationToken cancellationToken)
    {
        return await BuildLatestResultQuery(tenantId, companyId, year)
            .GroupBy(item => item.Scope)
            .Select(group => new ScopeDashboardItem(
                group.Key,
                group.Sum(item => item.TotalKgCo2e),
                group.Sum(item => item.TotalKgCo2e) / 1000m))
            .OrderBy(item => item.Scope)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlyEmissionsItem>> GetMonthlyTimelineAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        CancellationToken cancellationToken)
    {
        return await BuildLatestResultQuery(tenantId, companyId, year)
            .GroupBy(item => new { item.Year, item.Month })
            .Select(group => new MonthlyEmissionsItem(
                group.Key.Year,
                group.Key.Month,
                group.Sum(item => item.TotalKgCo2e),
                group.Sum(item => item.TotalKgCo2e) / 1000m))
            .OrderBy(item => item.Year)
            .ThenBy(item => item.Month)
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<ReportResultProjection> BuildLatestResultQuery(Guid tenantId, Guid? companyId, int? year)
    {
        var latestRuns =
            from run in dbContext.CalculationRuns.AsNoTracking()
            where run.TenantId == tenantId
            where !dbContext.CalculationRuns.Any(other =>
                other.TenantId == tenantId
                && other.InventoryId == run.InventoryId
                && other.CreatedAt > run.CreatedAt)
            select run;

        var query =
            from result in dbContext.CalculationResults.AsNoTracking()
            join run in latestRuns on result.CalculationRunId equals run.Id
            join inventory in dbContext.EmissionInventories.AsNoTracking() on run.InventoryId equals inventory.Id
            where result.TenantId == tenantId
            select new ReportResultProjection(
                inventory.CompanyId,
                inventory.Year,
                inventory.Month,
                result.Scope,
                result.TotalKgCo2e);

        if (companyId.HasValue)
        {
            query = query.Where(item => item.CompanyId == companyId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(item => item.Year == year.Value);
        }

        return query;
    }

    private sealed record ReportResultProjection(
        Guid CompanyId,
        int Year,
        int? Month,
        Domain.Enums.EmissionScope Scope,
        decimal TotalKgCo2e);
}
