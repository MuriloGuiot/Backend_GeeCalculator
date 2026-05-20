using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Inventories;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository(GeeCalculatorDbContext dbContext) : IInventoryRepository
{
    public Task<Domain.Entities.Company?> GetCompanyAsync(
        Guid tenantId,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        return dbContext.Companies.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == companyId,
            cancellationToken);
    }

    public Task<Domain.Entities.EmissionInventory?> GetAsync(
        Guid tenantId,
        Guid companyId,
        PeriodType periodType,
        int year,
        int? month,
        CancellationToken cancellationToken)
    {
        return dbContext.EmissionInventories.SingleOrDefaultAsync(
            item => item.TenantId == tenantId
                && item.CompanyId == companyId
                && item.PeriodType == periodType
                && item.Year == year
                && item.Month == month,
            cancellationToken);
    }

    public Task AddAsync(Domain.Entities.EmissionInventory inventory, CancellationToken cancellationToken)
    {
        return dbContext.EmissionInventories.AddAsync(inventory, cancellationToken).AsTask();
    }

    public void AddAuditLog(Domain.Entities.AuditLog auditLog)
    {
        dbContext.AuditLogs.Add(auditLog);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<InventorySummaryItem?> GetSummaryAsync(
        Guid tenantId,
        Guid inventoryId,
        CancellationToken cancellationToken)
    {
        return BuildSummaryQuery(tenantId, inventoryId: inventoryId)
            .SingleOrDefaultAsync(cancellationToken);
    }

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
        return await BuildSummaryQuery(tenantId, companyId, year, periodType)
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
            where entry.TenantId == tenantId && entry.InventoryId == inventoryId && entry.DeletedAt == null
            orderby entry.CreatedAt
            select new InventoryActivityEntryResponse(
                entry.Id,
                category.Code,
                category.Name,
                category.Scope,
                unit.Code,
                entry.ActivityValue,
                entry.SourceName,
                entry.CalculationMethod,
                entry.EvidenceRef,
                entry.MetadataJson,
                entry.CreatedAt,
                entry.UpdatedAt)).ToArrayAsync(cancellationToken);

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
                result.TotalKgCo2e,
                result.BiogenicKgCo2,
                result.BiogenicRemovalKgCo2
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
                    .GroupBy(result => result.Scope)
                    .OrderBy(group => group.Key)
                    .Select(group =>
                    {
                        var totalKg = group.Sum(result => result.TotalKgCo2e);
                        var biogenicKg = group.Sum(result => result.BiogenicKgCo2);
                        var biogenicRemovalKg = group.Sum(result => result.BiogenicRemovalKgCo2);

                        return new InventoryScopeResultResponse(
                            group.Key,
                            totalKg,
                            totalKg / 1000m,
                            biogenicKg,
                            biogenicKg / 1000m,
                            biogenicRemovalKg,
                            biogenicRemovalKg / 1000m);
                    })
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

    private IQueryable<InventorySummaryItem> BuildSummaryQuery(
        Guid tenantId,
        Guid? companyId = null,
        int? year = null,
        PeriodType? periodType = null,
        Guid? inventoryId = null)
    {
        var query =
            from inventory in dbContext.EmissionInventories.AsNoTracking()
            join company in dbContext.Companies.AsNoTracking() on inventory.CompanyId equals company.Id
            where inventory.TenantId == tenantId
            select new
            {
                Inventory = inventory,
                Company = company
            };

        if (inventoryId.HasValue)
        {
            query = query.Where(item => item.Inventory.Id == inventoryId.Value);
        }

        if (companyId.HasValue)
        {
            query = query.Where(item => item.Inventory.CompanyId == companyId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(item => item.Inventory.Year == year.Value);
        }

        if (periodType.HasValue)
        {
            query = query.Where(item => item.Inventory.PeriodType == periodType.Value);
        }

        return
            from item in query
            select new InventorySummaryItem(
                item.Inventory.Id,
                item.Company.Id,
                item.Company.LegalName,
                item.Inventory.PeriodType,
                item.Inventory.Year,
                item.Inventory.Month,
                item.Inventory.CreatedAt,
                dbContext.CalculationResults
                    .Where(result => result.TenantId == tenantId
                        && dbContext.CalculationRuns.Any(run =>
                            run.TenantId == tenantId
                            && run.InventoryId == item.Inventory.Id
                            && run.Id == result.CalculationRunId
                            && !dbContext.CalculationRuns.Any(other =>
                                other.TenantId == tenantId
                                && other.InventoryId == item.Inventory.Id
                                && other.CreatedAt > run.CreatedAt)))
                    .Sum(result => (decimal?)result.TotalKgCo2e) ?? 0m);
    }
}
