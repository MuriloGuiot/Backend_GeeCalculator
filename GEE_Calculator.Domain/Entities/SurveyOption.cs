namespace GEE_Calculator.Domain.Entities;

public sealed class SurveyOption
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid QuestionId { get; init; }
    public required string Code { get; init; }
    public required string Label { get; set; }
    public string? Value { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
