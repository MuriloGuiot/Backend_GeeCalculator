namespace GEE_Calculator.Domain.Entities;

public sealed class CalculationRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid InventoryId { get; init; }
    public string CalculationVersion { get; init; } = "gee-v0";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
