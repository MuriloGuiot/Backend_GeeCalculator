using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Calculations;

public interface IEmissionCalculationRepository
{
    Task<Tenant?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken);
    Task<Company?> GetCompanyByExternalIdAsync(Guid tenantId, string externalCompanyId, CancellationToken cancellationToken);
    Task<Company?> GetCompanyByTaxIdAsync(Guid tenantId, string taxId, CancellationToken cancellationToken);
    Task AddCompanyAsync(Company company, CancellationToken cancellationToken);
    Task<EmissionInventory?> GetInventoryAsync(Guid tenantId, Guid companyId, PeriodType periodType, int year, int? month, CancellationToken cancellationToken);
    Task AddInventoryAsync(EmissionInventory inventory, CancellationToken cancellationToken);
    Task<EmissionFactorSet?> GetFactorSetByCodeAsync(string factorSetCode, CancellationToken cancellationToken);
    Task<EmissionFactorSet> GetLatestFactorSetAsync(CancellationToken cancellationToken);
    Task<EmissionCategory?> GetCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken);
    Task<ActivityUnit?> GetActivityUnitByCodeAsync(string activityUnitCode, CancellationToken cancellationToken);
    Task<EmissionFactor?> GetEmissionFactorAsync(Guid factorSetId, Guid categoryId, Guid activityUnitId, Guid tenantId, CancellationToken cancellationToken);
    Task AddActivityEntryAsync(ActivityEntry activityEntry, CancellationToken cancellationToken);
    Task AddCalculationRunAsync(CalculationRun calculationRun, CancellationToken cancellationToken);
    void AddCalculationResult(CalculationResult calculationResult);
    void AddAuditLog(AuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
