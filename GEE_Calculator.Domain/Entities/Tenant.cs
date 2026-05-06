namespace GEE_Calculator.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string ExternalTenantId { get; init; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
