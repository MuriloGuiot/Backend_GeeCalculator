using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.ActivityEntries;

public sealed record ActivityEntryResponse(
    Guid Id,
    Guid InventoryId,
    string CategoryCode,
    string CategoryName,
    EmissionScope Scope,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? SourceName,
    string CalculationMethod,
    string? EvidenceRef,
    string MetadataJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateActivityEntryRequest(
    string CategoryCode,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? SourceName,
    string? CalculationMethod,
    string? EvidenceRef,
    string? MetadataJson);

public sealed record UpdateActivityEntryRequest(
    string CategoryCode,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? SourceName,
    string? CalculationMethod,
    string? EvidenceRef,
    string? MetadataJson);

public interface IActivityEntryService
{
    Task<IReadOnlyCollection<ActivityEntryResponse>> ListAsync(Guid inventoryId, CancellationToken cancellationToken = default);
    Task<ActivityEntryResponse> CreateAsync(Guid inventoryId, CreateActivityEntryRequest request, CancellationToken cancellationToken = default);
    Task<ActivityEntryResponse?> UpdateAsync(Guid inventoryId, Guid entryId, UpdateActivityEntryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid inventoryId, Guid entryId, CancellationToken cancellationToken = default);
}
