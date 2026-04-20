using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Entities;

public sealed class ActivityEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid InventoryId { get; init; }
    public EmissionScope Scope { get; init; }
    public required string Category { get; init; }
    public required string ActivityUnit { get; init; }
    public decimal ActivityValue { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
