namespace GEE_Calculator.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public required string ActorExternalUserId { get; init; }
    public required string Action { get; init; }
    public required string EntityName { get; init; }
    public string? EntityId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
