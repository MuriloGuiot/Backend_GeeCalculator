using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class ActivityEntry : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid InventoryId { get; init; }
    public Guid CategoryId { get; set; }
    public Guid ActivityUnitId { get; set; }
    public decimal ActivityValue { get; set; }
    public string? SourceName { get; set; }
    public string CalculationMethod { get; set; } = "factor";
    public string? EvidenceRef { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
