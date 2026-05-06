namespace GEE_Calculator.Domain.Entities;

public sealed class EmissionFactorSource
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Code { get; init; }
    public required string Name { get; set; }
    public string? Publisher { get; set; }
    public string? SourceUrl { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
