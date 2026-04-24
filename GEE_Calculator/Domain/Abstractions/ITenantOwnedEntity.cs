namespace GEE_Calculator.Domain.Abstractions;

public interface ITenantOwnedEntity
{
    Guid TenantId { get; }
}
