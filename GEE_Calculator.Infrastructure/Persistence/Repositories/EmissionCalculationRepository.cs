using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class EmissionCalculationRepository(GeeCalculatorDbContext dbContext) : IEmissionCalculationRepository
{
    public Task<Tenant?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return dbContext.Tenants.SingleOrDefaultAsync(item => item.Id == tenantId, cancellationToken);
    }

    public Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        return dbContext.Tenants.AddAsync(tenant, cancellationToken).AsTask();
    }

    public Task<CalculationInventoryItem?> GetInventoryForCalculationAsync(
        Guid tenantId,
        Guid inventoryId,
        CancellationToken cancellationToken)
    {
        return (
            from inventory in dbContext.EmissionInventories.AsNoTracking()
            join company in dbContext.Companies.AsNoTracking() on inventory.CompanyId equals company.Id
            where inventory.TenantId == tenantId && inventory.Id == inventoryId
            select new CalculationInventoryItem(
                inventory.Id,
                inventory.TenantId,
                company.Id,
                company.LegalName,
                inventory.PeriodType,
                inventory.Year,
                inventory.Month))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<Company?> GetCompanyByExternalIdAsync(Guid tenantId, string externalCompanyId, CancellationToken cancellationToken)
    {
        return dbContext.Companies.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.ExternalCompanyId == externalCompanyId,
            cancellationToken);
    }

    public Task<Company?> GetCompanyByTaxIdAsync(Guid tenantId, string taxId, CancellationToken cancellationToken)
    {
        return dbContext.Companies.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.TaxId == taxId,
            cancellationToken);
    }

    public Task AddCompanyAsync(Company company, CancellationToken cancellationToken)
    {
        return dbContext.Companies.AddAsync(company, cancellationToken).AsTask();
    }

    public Task<EmissionInventory?> GetInventoryAsync(
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

    public Task AddInventoryAsync(EmissionInventory inventory, CancellationToken cancellationToken)
    {
        return dbContext.EmissionInventories.AddAsync(inventory, cancellationToken).AsTask();
    }

    public Task<EmissionFactorSet?> GetFactorSetByCodeAsync(string factorSetCode, CancellationToken cancellationToken)
    {
        return dbContext.EmissionFactorSets.SingleOrDefaultAsync(item => item.Code == factorSetCode, cancellationToken);
    }

    public Task<EmissionFactorSet> GetLatestFactorSetAsync(CancellationToken cancellationToken)
    {
        return dbContext.EmissionFactorSets
            .OrderByDescending(item => item.VersionYear)
            .ThenBy(item => item.Code)
            .FirstAsync(cancellationToken);
    }

    public Task<EmissionCategory?> GetCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken)
    {
        return dbContext.EmissionCategories.SingleOrDefaultAsync(item => item.Code == categoryCode, cancellationToken);
    }

    public Task<ActivityUnit?> GetActivityUnitByCodeAsync(string activityUnitCode, CancellationToken cancellationToken)
    {
        return dbContext.ActivityUnits.SingleOrDefaultAsync(item => item.Code == activityUnitCode, cancellationToken);
    }

    public Task<EmissionFactor?> GetEmissionFactorAsync(
        Guid factorSetId,
        Guid categoryId,
        Guid activityUnitId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return dbContext.EmissionFactors
            .Where(item => item.FactorSetId == factorSetId
                && item.CategoryId == categoryId
                && item.ActivityUnitId == activityUnitId
                && (item.TenantId == null || item.TenantId == tenantId))
            .OrderByDescending(item => item.TenantId.HasValue)
            .ThenByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CalculationActivityEntry>> ListActiveEntriesAsync(
        Guid tenantId,
        Guid inventoryId,
        CancellationToken cancellationToken)
    {
        return await (
            from entry in dbContext.ActivityEntries.AsNoTracking()
            join category in dbContext.EmissionCategories.AsNoTracking() on entry.CategoryId equals category.Id
            join unit in dbContext.ActivityUnits.AsNoTracking() on entry.ActivityUnitId equals unit.Id
            where entry.TenantId == tenantId
                && entry.InventoryId == inventoryId
                && entry.DeletedAt == null
            orderby entry.CreatedAt, entry.Id
            select new CalculationActivityEntry(
                entry.Id,
                category.Id,
                category.Code,
                category.Name,
                category.Scope,
                unit.Id,
                unit.Code,
                entry.ActivityValue,
                entry.CalculationMethod,
                entry.EvidenceRef,
                entry.MetadataJson))
            .ToArrayAsync(cancellationToken);
    }

    public Task AddActivityEntryAsync(ActivityEntry activityEntry, CancellationToken cancellationToken)
    {
        return dbContext.ActivityEntries.AddAsync(activityEntry, cancellationToken).AsTask();
    }

    public Task AddCalculationRunAsync(CalculationRun calculationRun, CancellationToken cancellationToken)
    {
        return dbContext.CalculationRuns.AddAsync(calculationRun, cancellationToken).AsTask();
    }

    public void AddCalculationResult(CalculationResult calculationResult)
    {
        dbContext.CalculationResults.Add(calculationResult);
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        dbContext.AuditLogs.Add(auditLog);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
