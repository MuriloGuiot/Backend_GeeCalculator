using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Companies;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Application.Companies;

public sealed class CompanyService(
    ICompanyRepository companyRepository,
    ICurrentTenantAccessor currentTenantAccessor) : ICompanyService
{
    public async Task<PagedResponse<CompanySummaryResponse>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var normalizedPage = NormalizePage(page);
        var normalizedPageSize = NormalizePageSize(pageSize);
        var total = await companyRepository.CountAsync(tenantId, cancellationToken);
        var companies = await companyRepository.ListAsync(
            tenantId,
            (normalizedPage - 1) * normalizedPageSize,
            normalizedPageSize,
            cancellationToken);

        return new PagedResponse<CompanySummaryResponse>(
            companies.Select(ToSummary).ToArray(),
            normalizedPage,
            normalizedPageSize,
            total);
    }

    public async Task<CompanyDetailsResponse?> GetAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var company = await companyRepository.GetAsync(tenantId, companyId, cancellationToken);

        return company is null ? null : await ToDetailsAsync(company, cancellationToken);
    }

    public async Task<CompanyDetailsResponse> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.LegalName);
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);

        var existingCompany = await FindExistingCompanyAsync(tenantId, request.ExternalCompanyId, request.TaxId, cancellationToken);

        if (existingCompany is not null)
        {
            return await ToDetailsAsync(existingCompany, cancellationToken);
        }

        var company = new Company
        {
            TenantId = tenantId,
            LegalName = request.LegalName.Trim(),
            TradeName = NormalizeOptional(request.TradeName),
            TaxId = NormalizeOptional(request.TaxId),
            ExternalCompanyId = NormalizeOptional(request.ExternalCompanyId)
        };

        await companyRepository.AddAsync(company, cancellationToken);
        await companyRepository.SaveChangesAsync(cancellationToken);

        return await ToDetailsAsync(company, cancellationToken);
    }

    public async Task<CompanyDetailsResponse?> UpdateAsync(
        Guid companyId,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request.LegalName);
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var company = await companyRepository.GetAsync(tenantId, companyId, cancellationToken);

        if (company is null)
        {
            return null;
        }

        company.LegalName = request.LegalName.Trim();
        company.TradeName = NormalizeOptional(request.TradeName);
        company.TaxId = NormalizeOptional(request.TaxId);
        company.ExternalCompanyId = NormalizeOptional(request.ExternalCompanyId);

        await companyRepository.SaveChangesAsync(cancellationToken);

        return await ToDetailsAsync(company, cancellationToken);
    }

    private async Task<Company?> FindExistingCompanyAsync(
        Guid tenantId,
        string? externalCompanyId,
        string? taxId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(externalCompanyId))
        {
            var company = await companyRepository.GetByExternalIdAsync(tenantId, externalCompanyId.Trim(), cancellationToken);

            if (company is not null)
            {
                return company;
            }
        }

        return string.IsNullOrWhiteSpace(taxId)
            ? null
            : await companyRepository.GetByTaxIdAsync(tenantId, taxId.Trim(), cancellationToken);
    }

    private async Task<CompanyDetailsResponse> ToDetailsAsync(Company company, CancellationToken cancellationToken)
    {
        var inventoryCount = await companyRepository.CountInventoriesAsync(company.TenantId, company.Id, cancellationToken);
        var lastInventoryCreatedAt = await companyRepository.GetLastInventoryCreatedAtAsync(company.TenantId, company.Id, cancellationToken);

        return new CompanyDetailsResponse(
            company.Id,
            company.TenantId,
            company.LegalName,
            company.TradeName,
            company.TaxId,
            company.ExternalCompanyId,
            company.CreatedAt,
            inventoryCount,
            lastInventoryCreatedAt);
    }

    private static CompanySummaryResponse ToSummary(Company company)
    {
        return new CompanySummaryResponse(
            company.Id,
            company.TenantId,
            company.LegalName,
            company.TradeName,
            company.TaxId,
            company.ExternalCompanyId,
            company.CreatedAt);
    }

    private static void Validate(string legalName)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            throw new ArgumentException("LegalName is required.", nameof(legalName));
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => pageSize
    };
}
