using GEE_Calculator.Domain.EmissionFactors;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Infrastructure.Persistence.Repositories;

public sealed class EmissionFactorCatalogRepository(GeeCalculatorDbContext dbContext) : IEmissionFactorCatalogRepository
{
    public async Task<IReadOnlyCollection<EmissionCategory>> ListCategoriesAsync(
        EmissionScope? scope,
        CancellationToken cancellationToken)
    {
        var query = dbContext.EmissionCategories.AsNoTracking();

        if (scope.HasValue)
        {
            query = query.Where(item => item.Scope == scope.Value);
        }

        return await query
            .OrderBy(item => item.Scope)
            .ThenBy(item => item.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActivityUnit>> ListActivityUnitsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ActivityUnits
            .AsNoTracking()
            .OrderBy(item => item.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EmissionFactorSet>> ListFactorSetsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.EmissionFactorSets
            .AsNoTracking()
            .OrderByDescending(item => item.VersionYear)
            .ThenBy(item => item.Code)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountFactorsAsync(
        Guid tenantId,
        string? factorSetCode,
        EmissionScope? scope,
        CancellationToken cancellationToken)
    {
        return BuildFactorQuery(tenantId, factorSetCode, scope).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EmissionFactorCatalogItem>> ListFactorsAsync(
        Guid tenantId,
        string? factorSetCode,
        EmissionScope? scope,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await BuildFactorQuery(tenantId, factorSetCode, scope)
            .OrderBy(item => item.Scope)
            .ThenBy(item => item.CategoryCode)
            .ThenBy(item => item.ActivityUnitCode)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<EmissionFactorCatalogItem> BuildFactorQuery(Guid tenantId, string? factorSetCode, EmissionScope? scope)
    {
        var query =
            from factor in dbContext.EmissionFactors.AsNoTracking()
            join factorSet in dbContext.EmissionFactorSets.AsNoTracking() on factor.FactorSetId equals factorSet.Id
            join category in dbContext.EmissionCategories.AsNoTracking() on factor.CategoryId equals category.Id
            join unit in dbContext.ActivityUnits.AsNoTracking() on factor.ActivityUnitId equals unit.Id
            where factor.TenantId == null || factor.TenantId == tenantId
            select new EmissionFactorCatalogItem(
                factor.Id,
                factorSet.Code,
                category.Scope,
                category.Code,
                category.Name,
                unit.Code,
                factor.FactorKgPerUnit,
                factor.Gwp,
                factor.FactorKgCo2ePerUnit,
                factor.CalculationNotes,
                factor.TenantId.HasValue);

        if (!string.IsNullOrWhiteSpace(factorSetCode))
        {
            query = query.Where(item => item.FactorSetCode == factorSetCode);
        }

        if (scope.HasValue)
        {
            query = query.Where(item => item.Scope == scope.Value);
        }

        return query;
    }
}
