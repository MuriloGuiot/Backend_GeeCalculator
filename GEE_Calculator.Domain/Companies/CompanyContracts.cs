using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Entities;

namespace GEE_Calculator.Domain.Companies;

public sealed record CompanySummaryResponse(
    Guid Id,
    Guid TenantId,
    string LegalName,
    string? TradeName,
    string? TaxId,
    string? ExternalCompanyId,
    DateTimeOffset CreatedAt);

public sealed record CompanyDetailsResponse(
    Guid Id,
    Guid TenantId,
    string LegalName,
    string? TradeName,
    string? TaxId,
    string? ExternalCompanyId,
    DateTimeOffset CreatedAt,
    int InventoryCount,
    DateTimeOffset? LastInventoryCreatedAt);

public sealed record CreateCompanyRequest(
    string LegalName,
    string? TradeName,
    string? TaxId,
    string? ExternalCompanyId);

public sealed record UpdateCompanyRequest(
    string LegalName,
    string? TradeName,
    string? TaxId,
    string? ExternalCompanyId);

public interface ICompanyService
{
    Task<PagedResponse<CompanySummaryResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<CompanyDetailsResponse?> GetAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<CompanyDetailsResponse> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default);
    Task<CompanyDetailsResponse?> UpdateAsync(Guid companyId, UpdateCompanyRequest request, CancellationToken cancellationToken = default);
}

public interface ICompanyRepository
{
    Task<int> CountAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Company>> ListAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken);
    Task<Company?> GetAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken);
    Task<Company?> GetByExternalIdAsync(Guid tenantId, string externalCompanyId, CancellationToken cancellationToken);
    Task<Company?> GetByTaxIdAsync(Guid tenantId, string taxId, CancellationToken cancellationToken);
    Task<int> CountInventoriesAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken);
    Task<DateTimeOffset?> GetLastInventoryCreatedAtAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken);
    Task AddAsync(Company company, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
