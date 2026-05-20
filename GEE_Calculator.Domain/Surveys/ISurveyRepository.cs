using GEE_Calculator.Domain.Entities;

namespace GEE_Calculator.Domain.Surveys;

public interface ISurveyRepository
{
    Task<IReadOnlyCollection<SurveyTemplate>> ListActiveTemplatesAsync(CancellationToken cancellationToken);
    Task<SurveyTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SurveySection>> ListSectionsAsync(Guid templateId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SurveyQuestion>> ListQuestionsAsync(Guid templateId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SurveyOption>> ListOptionsAsync(Guid templateId, CancellationToken cancellationToken);
}
