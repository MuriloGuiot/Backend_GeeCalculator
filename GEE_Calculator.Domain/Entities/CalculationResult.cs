using GEE_Calculator.Domain.Abstractions;
using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Entities;

public sealed class CalculationResult : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid CalculationRunId { get; init; }
    public Guid? ActivityEntryId { get; init; }
    public EmissionScope Scope { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? GasId { get; init; }
    public Guid? EmissionFactorId { get; init; }
    public Guid? ActivityUnitId { get; init; }
    public decimal? ActivityValue { get; init; }
    public decimal? FactorKgCo2ePerUnit { get; init; }
    public decimal TotalKgCo2e { get; init; }
    public decimal TotalTCo2e => TotalKgCo2e / 1000m;
    public decimal BiogenicKgCo2 { get; init; }
    public decimal BiogenicTCo2 => BiogenicKgCo2 / 1000m;
    public decimal BiogenicRemovalKgCo2 { get; init; }
    public decimal BiogenicRemovalTCo2 => BiogenicRemovalKgCo2 / 1000m;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
