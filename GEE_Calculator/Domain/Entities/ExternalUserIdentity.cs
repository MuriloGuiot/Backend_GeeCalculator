using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class ExternalUserIdentity : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public required string Provider { get; init; }
    public required string ExternalUserId { get; init; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
