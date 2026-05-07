using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Companies;
using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.WebApi.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(ICompanyService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CompanySummaryResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await companyService.ListAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{companyId:guid}")]
    public async Task<ActionResult<CompanyDetailsResponse>> Get(Guid companyId, CancellationToken cancellationToken)
    {
        var company = await companyService.GetAsync(companyId, cancellationToken);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDetailsResponse>> Create(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await companyService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { companyId = company.Id }, company);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.ParamName,
                message = exception.Message
            });
        }
    }

    [HttpPut("{companyId:guid}")]
    public async Task<ActionResult<CompanyDetailsResponse>> Update(
        Guid companyId,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await companyService.UpdateAsync(companyId, request, cancellationToken);
            return company is null ? NotFound() : Ok(company);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.ParamName,
                message = exception.Message
            });
        }
    }
}
