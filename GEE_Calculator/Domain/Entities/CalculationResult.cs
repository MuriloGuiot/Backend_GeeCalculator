using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Entities;

public sealed class CalculationResult
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid CalculationRunId { get; init; }
    public EmissionScope Scope { get; init; }
    public decimal TotalKgCo2e { get; init; }
    public decimal TotalTCo2e => TotalKgCo2e / 1000m;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
