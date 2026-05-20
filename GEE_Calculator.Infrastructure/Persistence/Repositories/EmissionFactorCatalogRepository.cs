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
        var factorQuery = BuildFactorQuery(tenantId, factorSetCode, scope);
        var query =
            from factor in factorQuery
            join factorSet in dbContext.EmissionFactorSets.AsNoTracking() on factor.FactorSetId equals factorSet.Id
            join category in dbContext.EmissionCategories.AsNoTracking() on factor.CategoryId equals category.Id
            join unit in dbContext.ActivityUnits.AsNoTracking() on factor.ActivityUnitId equals unit.Id
            select new
            {
                Factor = factor,
                FactorSet = factorSet,
                Category = category,
                Unit = unit
            };

        return await query
            .OrderBy(item => item.Category.Scope)
            .ThenBy(item => item.Category.Code)
            .ThenBy(item => item.Unit.Code)
            .Skip(skip)
            .Take(take)
            .Select(item => new EmissionFactorCatalogItem(
                item.Factor.Id,
                item.FactorSet.Code,
                item.Category.Scope,
                item.Category.Code,
                item.Category.Name,
                item.Unit.Code,
                item.Factor.FactorKgPerUnit,
                item.Factor.Gwp,
                item.Factor.FactorKgCo2ePerUnit,
                item.Factor.CalculationNotes,
                item.Factor.TenantId.HasValue))
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<EmissionFactor> BuildFactorQuery(
        Guid tenantId,
        string? factorSetCode,
        EmissionScope? scope)
    {
        var query = dbContext.EmissionFactors
            .AsNoTracking()
            .Where(factor => factor.TenantId == null || factor.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(factorSetCode))
        {
            query = query.Where(factor => dbContext.EmissionFactorSets
                .Any(factorSet => factorSet.Id == factor.FactorSetId && factorSet.Code == factorSetCode));
        }

        if (scope.HasValue)
        {
            query = query.Where(factor => dbContext.EmissionCategories
                .Any(category => category.Id == factor.CategoryId && category.Scope == scope.Value));
        }

        return query;
    }
}
