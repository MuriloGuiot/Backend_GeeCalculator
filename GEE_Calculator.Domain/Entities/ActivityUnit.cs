namespace GEE_Calculator.Domain.Entities;

public sealed class ActivityUnit
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Code { get; init; }
    public required string Name { get; set; }
    public required string Dimension { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
