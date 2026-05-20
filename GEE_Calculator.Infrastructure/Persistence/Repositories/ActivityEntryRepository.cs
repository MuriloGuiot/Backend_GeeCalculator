using GEE_Calculator.Domain.ActivityEntries;
using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class ActivityEntryRepository(GeeCalculatorDbContext dbContext) : IActivityEntryRepository
{
    public Task<bool> InventoryExistsAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken)
    {
        return dbContext.EmissionInventories.AnyAsync(
            item => item.TenantId == tenantId && item.Id == inventoryId,
            cancellationToken);
    }

    public Task<EmissionCategory?> GetCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken)
    {
        return dbContext.EmissionCategories.SingleOrDefaultAsync(
            item => item.Code == categoryCode,
            cancellationToken);
    }

    public Task<ActivityUnit?> GetActivityUnitByCodeAsync(string activityUnitCode, CancellationToken cancellationToken)
    {
        return dbContext.ActivityUnits.SingleOrDefaultAsync(
            item => item.Code == activityUnitCode,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActivityEntryListItem>> ListAsync(
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
            select new ActivityEntryListItem(entry, category, unit))
            .ToArrayAsync(cancellationToken);
    }

    public Task<ActivityEntry?> GetAsync(
        Guid tenantId,
        Guid inventoryId,
        Guid entryId,
        CancellationToken cancellationToken)
    {
        return dbContext.ActivityEntries.SingleOrDefaultAsync(
            item => item.TenantId == tenantId
                && item.InventoryId == inventoryId
                && item.Id == entryId
                && item.DeletedAt == null,
            cancellationToken);
    }

    public Task AddAsync(ActivityEntry activityEntry, CancellationToken cancellationToken)
    {
        return dbContext.ActivityEntries.AddAsync(activityEntry, cancellationToken).AsTask();
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
