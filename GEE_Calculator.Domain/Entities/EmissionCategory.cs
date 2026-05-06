using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Entities;

public sealed class EmissionCategory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public EmissionScope Scope { get; init; }
    public required string Code { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
