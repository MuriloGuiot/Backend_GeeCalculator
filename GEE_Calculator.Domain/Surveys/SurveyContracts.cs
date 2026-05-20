namespace GEE_Calculator.Domain.Surveys;

public sealed record SurveyTemplateSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    string VersionLabel,
    bool IsActive);

public sealed record SurveyTemplateDetailsResponse(
    Guid Id,
    string Code,
    string Name,
    string VersionLabel,
    bool IsActive,
    IReadOnlyCollection<SurveySectionResponse> Sections);

public sealed record SurveySectionResponse(
    Guid Id,
    string Code,
    string Title,
    string? Description,
    int SortOrder,
    IReadOnlyCollection<SurveyQuestionResponse> Questions);

public sealed record SurveyQuestionResponse(
    Guid Id,
    string Code,
    string Prompt,
    string? HelpText,
    string AnswerType,
    bool IsRequired,
    int SortOrder,
    string VisibilityRuleJson,
    string MappingJson,
    IReadOnlyCollection<SurveyOptionResponse> Options);

public sealed record SurveyOptionResponse(
    Guid Id,
    string Code,
    string Label,
    string? Value,
    int SortOrder);

public interface ISurveyService
{
    Task<IReadOnlyCollection<SurveyTemplateSummaryResponse>> ListTemplatesAsync(CancellationToken cancellationToken = default);
    Task<SurveyTemplateDetailsResponse?> GetTemplateAsync(string code, CancellationToken cancellationToken = default);
}
