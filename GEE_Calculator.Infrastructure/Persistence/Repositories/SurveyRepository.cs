using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Surveys;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class SurveyRepository(GeeCalculatorDbContext dbContext) : ISurveyRepository
{
    public async Task<IReadOnlyCollection<SurveyTemplate>> ListActiveTemplatesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SurveyTemplates
            .AsNoTracking()
            .Where(template => template.IsActive)
            .OrderBy(template => template.Code)
            .ToArrayAsync(cancellationToken);
    }

    public Task<SurveyTemplate?> GetTemplateByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return dbContext.SurveyTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(template => template.Code == code && template.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SurveySection>> ListSectionsAsync(Guid templateId, CancellationToken cancellationToken)
    {
        return await dbContext.SurveySections
            .AsNoTracking()
            .Where(section => section.TemplateId == templateId)
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SurveyQuestion>> ListQuestionsAsync(Guid templateId, CancellationToken cancellationToken)
    {
        return await (
            from question in dbContext.SurveyQuestions.AsNoTracking()
            join section in dbContext.SurveySections.AsNoTracking() on question.SectionId equals section.Id
            where section.TemplateId == templateId
            orderby section.SortOrder, question.SortOrder, question.Code
            select question)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SurveyOption>> ListOptionsAsync(Guid templateId, CancellationToken cancellationToken)
    {
        return await (
            from option in dbContext.SurveyOptions.AsNoTracking()
            join question in dbContext.SurveyQuestions.AsNoTracking() on option.QuestionId equals question.Id
            join section in dbContext.SurveySections.AsNoTracking() on question.SectionId equals section.Id
            where section.TemplateId == templateId
            orderby question.SortOrder, option.SortOrder, option.Code
            select option)
            .ToArrayAsync(cancellationToken);
    }
}
