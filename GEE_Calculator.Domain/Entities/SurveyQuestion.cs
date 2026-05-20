namespace GEE_Calculator.Domain.Entities;

public sealed class SurveyQuestion
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid SectionId { get; init; }
    public required string Code { get; init; }
    public required string Prompt { get; set; }
    public string? HelpText { get; set; }
    public required string AnswerType { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string VisibilityRuleJson { get; set; } = "{}";
    public string MappingJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
