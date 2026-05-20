using GEE_Calculator.Domain.Entities;

namespace GEE_Calculator.Domain.ActivityEntries;

public interface IActivityEntryRepository
{
    Task<bool> InventoryExistsAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken);
    Task<EmissionCategory?> GetCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken);
    Task<ActivityUnit?> GetActivityUnitByCodeAsync(string activityUnitCode, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ActivityEntryListItem>> ListAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken);
    Task<ActivityEntry?> GetAsync(Guid tenantId, Guid inventoryId, Guid entryId, CancellationToken cancellationToken);
    Task AddAsync(ActivityEntry activityEntry, CancellationToken cancellationToken);
    void AddAuditLog(AuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record ActivityEntryListItem(
    ActivityEntry Entry,
    EmissionCategory Category,
    ActivityUnit ActivityUnit);
