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
        var results = await BuildLatestResultQuery(tenantId, companyId, year)
            .ToArrayAsync(cancellationToken);

        return results
            .GroupBy(item => item.Scope)
            .Select(group => new ScopeDashboardItem(
                group.Key,
                group.Sum(item => item.TotalKgCo2e),
                group.Sum(item => item.TotalKgCo2e) / 1000m,
                group.Sum(item => item.BiogenicKgCo2),
                group.Sum(item => item.BiogenicKgCo2) / 1000m,
                group.Sum(item => item.BiogenicRemovalKgCo2),
                group.Sum(item => item.BiogenicRemovalKgCo2) / 1000m))
            .OrderBy(item => item.Scope)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<CategoryDashboardItem>> GetCategoryTotalsAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        CancellationToken cancellationToken)
    {
        var results = await BuildLatestResultQuery(tenantId, companyId, year)
            .ToArrayAsync(cancellationToken);

        return results
            .Where(item => item.CategoryCode != null && item.CategoryName != null)
            .GroupBy(item => new
            {
                item.Scope,
                item.CategoryCode,
                item.CategoryName
            })
            .Select(group => new CategoryDashboardItem(
                group.Key.Scope,
                group.Key.CategoryCode!,
                group.Key.CategoryName!,
                group.Sum(item => item.TotalKgCo2e),
                group.Sum(item => item.TotalKgCo2e) / 1000m,
                group.Sum(item => item.BiogenicKgCo2),
                group.Sum(item => item.BiogenicKgCo2) / 1000m,
                group.Sum(item => item.BiogenicRemovalKgCo2),
                group.Sum(item => item.BiogenicRemovalKgCo2) / 1000m))
            .OrderBy(item => item.Scope)
            .ThenBy(item => item.CategoryCode)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<MonthlyEmissionsItem>> GetMonthlyTimelineAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        CancellationToken cancellationToken)
    {
        var results = await BuildLatestResultQuery(tenantId, companyId, year)
            .ToArrayAsync(cancellationToken);

        return results
            .GroupBy(item => new { item.Year, item.Month })
            .Select(group => new MonthlyEmissionsItem(
                group.Key.Year,
                group.Key.Month,
                group.Sum(item => item.TotalKgCo2e),
                group.Sum(item => item.TotalKgCo2e) / 1000m,
                group.Sum(item => item.BiogenicKgCo2),
                group.Sum(item => item.BiogenicKgCo2) / 1000m,
                group.Sum(item => item.BiogenicRemovalKgCo2),
                group.Sum(item => item.BiogenicRemovalKgCo2) / 1000m))
            .OrderBy(item => item.Year)
            .ThenBy(item => item.Month)
            .ToArray();
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
            join category in dbContext.EmissionCategories.AsNoTracking() on result.CategoryId equals category.Id into categoryJoin
            from category in categoryJoin.DefaultIfEmpty()
            where result.TenantId == tenantId
            select new
            {
                Result = result,
                Inventory = inventory,
                Category = category
            };

        if (companyId.HasValue)
        {
            query = query.Where(item => item.Inventory.CompanyId == companyId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(item => item.Inventory.Year == year.Value);
        }

        return query.Select(item => new ReportResultProjection(
            item.Inventory.CompanyId,
            item.Inventory.Year,
            item.Inventory.Month,
            item.Result.Scope,
            item.Category == null ? null : item.Category.Code,
            item.Category == null ? null : item.Category.Name,
            item.Result.TotalKgCo2e,
            item.Result.BiogenicKgCo2,
            item.Result.BiogenicRemovalKgCo2));
    }

    private sealed record ReportResultProjection(
        Guid CompanyId,
        int Year,
        int? Month,
        Domain.Enums.EmissionScope Scope,
        string? CategoryCode,
        string? CategoryName,
        decimal TotalKgCo2e,
        decimal BiogenicKgCo2,
        decimal BiogenicRemovalKgCo2);
}
