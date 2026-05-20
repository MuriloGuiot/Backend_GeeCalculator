namespace GEE_Calculator.Domain.Entities;

public sealed class SurveySection
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TemplateId { get; init; }
    public required string Code { get; init; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
