using GEE_Calculator.Application.Companies;
using GEE_Calculator.Domain.Companies;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Tests;

public sealed class CompanyServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task CreateAsync_ShouldCreateCompanyForCurrentTenant()
    {
        var repository = new InMemoryCompanyRepository();
        var service = new CompanyService(repository, new FixedTenantAccessor(TenantId));

        var response = await service.CreateAsync(new CreateCompanyRequest(
            LegalName: "AgrocarbonBR Cliente",
            TradeName: "Cliente",
            TaxId: "12345678000199",
            ExternalCompanyId: "company-001"));

        Assert.Equal(TenantId, response.TenantId);
        Assert.Equal("AgrocarbonBR Cliente", response.LegalName);
        Assert.Single(repository.Companies);
    }

    [Fact]
    public async Task CreateAsync_ShouldReuseExistingCompanyByExternalId()
    {
        var repository = new InMemoryCompanyRepository();
        var service = new CompanyService(repository, new FixedTenantAccessor(TenantId));

        var first = await service.CreateAsync(new CreateCompanyRequest(
            LegalName: "AgrocarbonBR Cliente",
            TradeName: null,
            TaxId: "12345678000199",
            ExternalCompanyId: "company-001"));
        var second = await service.CreateAsync(new CreateCompanyRequest(
            LegalName: "AgrocarbonBR Cliente Atualizado",
            TradeName: null,
            TaxId: "99999999000199",
            ExternalCompanyId: "company-001"));

        Assert.Equal(first.Id, second.Id);
        Assert.Single(repository.Companies);
    }

    private sealed class FixedTenantAccessor(Guid tenantId) : ICurrentTenantAccessor
    {
        public CurrentTenantSnapshot GetCurrentTenant()
        {
            return new CurrentTenantSnapshot(tenantId.ToString(), null, null, true);
        }
    }

    private sealed class InMemoryCompanyRepository : ICompanyRepository
    {
        public List<Company> Companies { get; } = [];

        public Task<int> CountAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Companies.Count(item => item.TenantId == tenantId));
        }

        public Task<IReadOnlyCollection<Company>> ListAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Company>>(
                Companies.Where(item => item.TenantId == tenantId).Skip(skip).Take(take).ToArray());
        }

        public Task<Company?> GetAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Companies.SingleOrDefault(item => item.TenantId == tenantId && item.Id == companyId));
        }

        public Task<Company?> GetByExternalIdAsync(Guid tenantId, string externalCompanyId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Companies.SingleOrDefault(item => item.TenantId == tenantId && item.ExternalCompanyId == externalCompanyId));
        }

        public Task<Company?> GetByTaxIdAsync(Guid tenantId, string taxId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Companies.SingleOrDefault(item => item.TenantId == tenantId && item.TaxId == taxId));
        }

        public Task<int> CountInventoriesAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<DateTimeOffset?> GetLastInventoryCreatedAtAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken)
        {
            return Task.FromResult<DateTimeOffset?>(null);
        }

        public Task AddAsync(Company company, CancellationToken cancellationToken)
        {
            Companies.Add(company);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
