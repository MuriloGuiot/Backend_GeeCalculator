using GEE_Calculator.Domain.Abstractions;
using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Entities;

public sealed class EmissionInventory : ITenantOwnedEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid CompanyId { get; init; }
    public PeriodType PeriodType { get; init; }
    public int Year { get; init; }
    public int? Month { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
