using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Inventories;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository(GeeCalculatorDbContext dbContext) : IInventoryRepository
{
    public Task<int> CountAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        PeriodType? periodType,
        CancellationToken cancellationToken)
    {
        return ApplyFilters(tenantId, companyId, year, periodType).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventorySummaryItem>> ListAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        PeriodType? periodType,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query =
            from inventory in ApplyFilters(tenantId, companyId, year, periodType).AsNoTracking()
            join company in dbContext.Companies.AsNoTracking() on inventory.CompanyId equals company.Id
            select new InventorySummaryItem(
                inventory.Id,
                company.Id,
                company.LegalName,
                inventory.PeriodType,
                inventory.Year,
                inventory.Month,
                inventory.CreatedAt,
                dbContext.CalculationResults
                    .Where(result => result.TenantId == tenantId
                        && dbContext.CalculationRuns.Any(run =>
                            run.TenantId == tenantId
                            && run.InventoryId == inventory.Id
                            && run.Id == result.CalculationRunId
                            && !dbContext.CalculationRuns.Any(other =>
                                other.TenantId == tenantId
                                && other.InventoryId == inventory.Id
                                && other.CreatedAt > run.CreatedAt)))
                    .Sum(result => (decimal?)result.TotalKgCo2e) ?? 0m);

        return await query
            .OrderByDescending(item => item.Year)
            .ThenByDescending(item => item.Month)
            .ThenBy(item => item.CompanyLegalName)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<InventoryDetailsItem?> GetDetailsAsync(
        Guid tenantId,
        Guid inventoryId,
        CancellationToken cancellationToken)
    {
        var inventory = await (
            from item in dbContext.EmissionInventories.AsNoTracking()
            join company in dbContext.Companies.AsNoTracking() on item.CompanyId equals company.Id
            where item.TenantId == tenantId && item.Id == inventoryId
            select new
            {
                item.Id,
                CompanyId = company.Id,
                CompanyLegalName = company.LegalName,
                item.PeriodType,
                item.Year,
                item.Month,
                item.CreatedAt
            }).SingleOrDefaultAsync(cancellationToken);

        if (inventory is null)
        {
            return null;
        }

        var entries = await (
            from entry in dbContext.ActivityEntries.AsNoTracking()
            join category in dbContext.EmissionCategories.AsNoTracking() on entry.CategoryId equals category.Id
            join unit in dbContext.ActivityUnits.AsNoTracking() on entry.ActivityUnitId equals unit.Id
            where entry.TenantId == tenantId && entry.InventoryId == inventoryId
            orderby entry.CreatedAt
            select new InventoryActivityEntryResponse(
                entry.Id,
                category.Code,
                category.Name,
                category.Scope,
                unit.Code,
                entry.ActivityValue,
                entry.EvidenceRef,
                entry.MetadataJson,
                entry.CreatedAt)).ToArrayAsync(cancellationToken);

        var runs = await (
            from run in dbContext.CalculationRuns.AsNoTracking()
            join factorSet in dbContext.EmissionFactorSets.AsNoTracking() on run.FactorSetId equals factorSet.Id
            where run.TenantId == tenantId && run.InventoryId == inventoryId
            orderby run.CreatedAt descending
            select new
            {
                run.Id,
                FactorSetCode = factorSet.Code,
                run.CalculationVersion,
                run.CreatedAt
            }).ToArrayAsync(cancellationToken);

        var runIds = runs.Select(run => run.Id).ToArray();
        var results = await dbContext.CalculationResults
            .AsNoTracking()
            .Where(result => result.TenantId == tenantId && runIds.Contains(result.CalculationRunId))
            .Select(result => new
            {
                result.CalculationRunId,
                result.Scope,
                result.TotalKgCo2e
            })
            .ToArrayAsync(cancellationToken);

        var runResponses = runs
            .Select(run => new InventoryCalculationRunResponse(
                run.Id,
                run.FactorSetCode,
                run.CalculationVersion,
                run.CreatedAt,
                results
                    .Where(result => result.CalculationRunId == run.Id)
                    .OrderBy(result => result.Scope)
                    .Select(result => new InventoryScopeResultResponse(
                        result.Scope,
                        result.TotalKgCo2e,
                        result.TotalKgCo2e / 1000m))
                    .ToArray()))
            .ToArray();

        return new InventoryDetailsItem(
            inventory.Id,
            inventory.CompanyId,
            inventory.CompanyLegalName,
            inventory.PeriodType,
            inventory.Year,
            inventory.Month,
            inventory.CreatedAt,
            entries,
            runResponses);
    }

    private IQueryable<Domain.Entities.EmissionInventory> ApplyFilters(
        Guid tenantId,
        Guid? companyId,
        int? year,
        PeriodType? periodType)
    {
        var query = dbContext.EmissionInventories.Where(item => item.TenantId == tenantId);

        if (companyId.HasValue)
        {
            query = query.Where(item => item.CompanyId == companyId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(item => item.Year == year.Value);
        }

        if (periodType.HasValue)
        {
            query = query.Where(item => item.PeriodType == periodType.Value);
        }

        return query;
    }
}
