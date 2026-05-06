using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class ApiClient : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public required string Name { get; set; }
    public required string KeyPrefix { get; init; }
    public required string KeyHash { get; init; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
}
