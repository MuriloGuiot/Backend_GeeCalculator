using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.EmissionFactors;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Application.EmissionFactors;

public sealed class EmissionFactorCatalogService(
    IEmissionFactorCatalogRepository catalogRepository,
    ICurrentTenantAccessor currentTenantAccessor) : IEmissionFactorCatalogService
{
    public async Task<IReadOnlyCollection<EmissionCategoryResponse>> ListCategoriesAsync(
        EmissionScope? scope,
        CancellationToken cancellationToken = default)
    {
        var categories = await catalogRepository.ListCategoriesAsync(scope, cancellationToken);
        return categories.Select(ToCategoryResponse).ToArray();
    }

    public async Task<IReadOnlyCollection<ActivityUnitResponse>> ListActivityUnitsAsync(CancellationToken cancellationToken = default)
    {
        var units = await catalogRepository.ListActivityUnitsAsync(cancellationToken);
        return units.Select(unit => new ActivityUnitResponse(unit.Id, unit.Code, unit.Name, unit.Dimension)).ToArray();
    }

    public async Task<IReadOnlyCollection<EmissionFactorSetResponse>> ListFactorSetsAsync(CancellationToken cancellationToken = default)
    {
        var factorSets = await catalogRepository.ListFactorSetsAsync(cancellationToken);
        return factorSets.Select(ToFactorSetResponse).ToArray();
    }

    public async Task<PagedResponse<EmissionFactorResponse>> ListFactorsAsync(
        string? factorSetCode,
        EmissionScope? scope,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            < 1 => 50,
            > 200 => 200,
            _ => pageSize
        };

        var total = await catalogRepository.CountFactorsAsync(tenantId, factorSetCode, scope, cancellationToken);
        var factors = await catalogRepository.ListFactorsAsync(
            tenantId,
            factorSetCode,
            scope,
            (normalizedPage - 1) * normalizedPageSize,
            normalizedPageSize,
            cancellationToken);

        return new PagedResponse<EmissionFactorResponse>(
            factors.Select(ToFactorResponse).ToArray(),
            normalizedPage,
            normalizedPageSize,
            total);
    }

    private static EmissionCategoryResponse ToCategoryResponse(EmissionCategory category)
    {
        return new EmissionCategoryResponse(
            category.Id,
            category.Scope,
            category.Code,
            category.Name,
            category.Description,
            category.ParentCategoryId);
    }

    private static EmissionFactorSetResponse ToFactorSetResponse(EmissionFactorSet factorSet)
    {
        return new EmissionFactorSetResponse(
            factorSet.Id,
            factorSet.Code,
            factorSet.Name,
            factorSet.VersionLabel,
            factorSet.VersionYear,
            factorSet.ValidFrom,
            factorSet.ValidTo);
    }

    private static EmissionFactorResponse ToFactorResponse(EmissionFactorCatalogItem item)
    {
        return new EmissionFactorResponse(
            item.Id,
            item.FactorSetCode,
            item.Scope,
            item.CategoryCode,
            item.CategoryName,
            item.ActivityUnitCode,
            item.FactorKgPerUnit,
            item.Gwp,
            item.FactorKgCo2ePerUnit,
            item.CalculationNotes,
            item.IsTenantSpecific);
    }
}
