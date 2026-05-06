namespace GEE_Calculator.Domain.Entities;

public sealed class EmissionFactor
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? TenantId { get; init; }
    public Guid FactorSetId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid ActivityUnitId { get; init; }
    public Guid? GasId { get; init; }
    public decimal? FactorKgPerUnit { get; init; }
    public decimal? Gwp { get; init; }
    public decimal FactorKgCo2ePerUnit { get; init; }
    public string? CalculationNotes { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
