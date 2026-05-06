using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class ActivityEntry : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid InventoryId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid ActivityUnitId { get; init; }
    public decimal ActivityValue { get; init; }
    public string? EvidenceRef { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
