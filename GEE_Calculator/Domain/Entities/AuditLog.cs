using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class AuditLog : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public string? ActorExternalUserId { get; init; }
    public required string Action { get; init; }
    public required string EntityName { get; init; }
    public string? EntityId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
