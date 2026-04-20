using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Entities;

public sealed class EmissionFactor
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public EmissionScope Scope { get; init; }
    public required string Category { get; init; }
    public required string Source { get; init; }
    public required string Unit { get; init; }
    public decimal FactorKgCo2e { get; init; }
    public decimal Gwp { get; init; } = 1m;
    public int VersionYear { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
