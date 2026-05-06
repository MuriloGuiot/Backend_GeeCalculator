using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class CalculationRun : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid InventoryId { get; init; }
    public Guid FactorSetId { get; init; }
    public string CalculationVersion { get; init; } = "gee-v0";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
