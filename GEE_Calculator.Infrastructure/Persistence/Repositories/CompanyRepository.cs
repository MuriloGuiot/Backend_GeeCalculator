using GEE_Calculator.Domain.Companies;
using GEE_Calculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository(GeeCalculatorDbContext dbContext) : ICompanyRepository
{
    public Task<int> CountAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return dbContext.Companies.CountAsync(item => item.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Company>> ListAsync(
        Guid tenantId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.LegalName)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Company?> GetAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken)
    {
        return dbContext.Companies.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == companyId,
            cancellationToken);
    }

    public Task<Company?> GetByExternalIdAsync(Guid tenantId, string externalCompanyId, CancellationToken cancellationToken)
    {
        return dbContext.Companies.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.ExternalCompanyId == externalCompanyId,
            cancellationToken);
    }

    public Task<Company?> GetByTaxIdAsync(Guid tenantId, string taxId, CancellationToken cancellationToken)
    {
        return dbContext.Companies.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.TaxId == taxId,
            cancellationToken);
    }

    public Task<int> CountInventoriesAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken)
    {
        return dbContext.EmissionInventories.CountAsync(
            item => item.TenantId == tenantId && item.CompanyId == companyId,
            cancellationToken);
    }

    public async Task<DateTimeOffset?> GetLastInventoryCreatedAtAsync(
        Guid tenantId,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        return await dbContext.EmissionInventories
            .Where(item => item.TenantId == tenantId && item.CompanyId == companyId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => (DateTimeOffset?)item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(Company company, CancellationToken cancellationToken)
    {
        return dbContext.Companies.AddAsync(company, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
