namespace GEE_Calculator.Domain.Entities;

public sealed class GreenhouseGas
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Code { get; init; }
    public required string Name { get; set; }
    public decimal DefaultGwp { get; init; }
    public required string GwpSource { get; init; }
    public int VersionYear { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
