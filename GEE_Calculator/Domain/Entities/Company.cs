using GEE_Calculator.Domain.Abstractions;

namespace GEE_Calculator.Domain.Entities;

public sealed class Company : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public required string LegalName { get; set; }
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string? ExternalCompanyId { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
