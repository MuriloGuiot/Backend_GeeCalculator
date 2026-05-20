namespace GEE_Calculator.Domain.Entities;

public sealed class SurveyTemplate
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Code { get; init; }
    public required string Name { get; set; }
    public required string VersionLabel { get; set; }
    public Guid? FactorSetId { get; init; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
