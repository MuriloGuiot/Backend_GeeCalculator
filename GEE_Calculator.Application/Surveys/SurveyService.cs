using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Surveys;

namespace GEE_Calculator.Application.Surveys;

public sealed class SurveyService(ISurveyRepository surveyRepository) : ISurveyService
{
    public async Task<IReadOnlyCollection<SurveyTemplateSummaryResponse>> ListTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        var templates = await surveyRepository.ListActiveTemplatesAsync(cancellationToken);
        return templates
            .OrderBy(template => template.Code)
            .Select(template => new SurveyTemplateSummaryResponse(
                template.Id,
                template.Code,
                template.Name,
                template.VersionLabel,
                template.IsActive))
            .ToArray();
    }

    public async Task<SurveyTemplateDetailsResponse?> GetTemplateAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var template = await surveyRepository.GetTemplateByCodeAsync(code.Trim(), cancellationToken);
        if (template is null)
        {
            return null;
        }

        var sections = await surveyRepository.ListSectionsAsync(template.Id, cancellationToken);
        var questions = await surveyRepository.ListQuestionsAsync(template.Id, cancellationToken);
        var options = await surveyRepository.ListOptionsAsync(template.Id, cancellationToken);

        var sectionResponses = sections
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Code)
            .Select(section => ToSectionResponse(
                section,
                questions.Where(question => question.SectionId == section.Id),
                options))
            .ToArray();

        return new SurveyTemplateDetailsResponse(
            template.Id,
            template.Code,
            template.Name,
            template.VersionLabel,
            template.IsActive,
            sectionResponses);
    }

    private static SurveySectionResponse ToSectionResponse(
        SurveySection section,
        IEnumerable<SurveyQuestion> questions,
        IReadOnlyCollection<SurveyOption> options)
    {
        var questionResponses = questions
            .OrderBy(question => question.SortOrder)
            .ThenBy(question => question.Code)
            .Select(question => new SurveyQuestionResponse(
                question.Id,
                question.Code,
                question.Prompt,
                question.HelpText,
                question.AnswerType,
                question.IsRequired,
                question.SortOrder,
                question.VisibilityRuleJson,
                question.MappingJson,
                options
                    .Where(option => option.QuestionId == question.Id)
                    .OrderBy(option => option.SortOrder)
                    .ThenBy(option => option.Code)
                    .Select(option => new SurveyOptionResponse(
                        option.Id,
                        option.Code,
                        option.Label,
                        option.Value,
                        option.SortOrder))
                    .ToArray()))
            .ToArray();

        return new SurveySectionResponse(
            section.Id,
            section.Code,
            section.Title,
            section.Description,
            section.SortOrder,
            questionResponses);
    }
}
