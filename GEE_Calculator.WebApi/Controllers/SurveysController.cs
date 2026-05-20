using GEE_Calculator.Domain.Surveys;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/surveys")]
public sealed class SurveysController(ISurveyService surveyService) : ControllerBase
{
    [HttpGet("templates")]
    public async Task<ActionResult<IReadOnlyCollection<SurveyTemplateSummaryResponse>>> ListTemplates(
        CancellationToken cancellationToken)
    {
        return Ok(await surveyService.ListTemplatesAsync(cancellationToken));
    }

    [HttpGet("templates/{code}")]
    public async Task<ActionResult<SurveyTemplateDetailsResponse>> GetTemplate(
        string code,
        CancellationToken cancellationToken)
    {
        var template = await surveyService.GetTemplateAsync(code, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }
}
