using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.EmissionFactors;

public sealed record EmissionCategoryResponse(
    Guid Id,
    EmissionScope Scope,
    string Code,
    string Name,
    string? Description,
    Guid? ParentCategoryId);

public sealed record ActivityUnitResponse(Guid Id, string Code, string Name, string Dimension);

public sealed record EmissionFactorSetResponse(
    Guid Id,
    string Code,
    string Name,
    string VersionLabel,
    int VersionYear,
    DateOnly? ValidFrom,
    DateOnly? ValidTo);

public sealed record EmissionFactorResponse(
    Guid Id,
    string FactorSetCode,
    EmissionScope Scope,
    string CategoryCode,
    string CategoryName,
    string ActivityUnitCode,
    decimal? FactorKgPerUnit,
    decimal? Gwp,
    decimal FactorKgCo2ePerUnit,
    string? CalculationNotes,
    bool IsTenantSpecific);

public interface IEmissionFactorCatalogService
{
    Task<IReadOnlyCollection<EmissionCategoryResponse>> ListCategoriesAsync(EmissionScope? scope, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ActivityUnitResponse>> ListActivityUnitsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<EmissionFactorSetResponse>> ListFactorSetsAsync(CancellationToken cancellationToken = default);
    Task<PagedResponse<EmissionFactorResponse>> ListFactorsAsync(
        string? factorSetCode,
        EmissionScope? scope,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public interface IEmissionFactorCatalogRepository
{
    Task<IReadOnlyCollection<EmissionCategory>> ListCategoriesAsync(EmissionScope? scope, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ActivityUnit>> ListActivityUnitsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmissionFactorSet>> ListFactorSetsAsync(CancellationToken cancellationToken);
    Task<int> CountFactorsAsync(Guid tenantId, string? factorSetCode, EmissionScope? scope, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmissionFactorCatalogItem>> ListFactorsAsync(
        Guid tenantId,
        string? factorSetCode,
        EmissionScope? scope,
        int skip,
        int take,
        CancellationToken cancellationToken);
}

public sealed record EmissionFactorCatalogItem(
    Guid Id,
    string FactorSetCode,
    EmissionScope Scope,
    string CategoryCode,
    string CategoryName,
    string ActivityUnitCode,
    decimal? FactorKgPerUnit,
    decimal? Gwp,
    decimal FactorKgCo2ePerUnit,
    string? CalculationNotes,
    bool IsTenantSpecific);
