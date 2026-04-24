namespace GEE_Calculator.Domain.Entities;

public sealed class EmissionFactorSet
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SourceId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; set; }
    public required string VersionLabel { get; init; }
    public int VersionYear { get; init; }
    public DateOnly? ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
