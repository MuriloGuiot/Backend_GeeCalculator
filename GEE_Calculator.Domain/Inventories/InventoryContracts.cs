using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Enums;

namespace GEE_Calculator.Domain.Inventories;

public sealed record InventorySummaryResponse(
    Guid Id,
    Guid CompanyId,
    string CompanyLegalName,
    PeriodType PeriodType,
    int Year,
    int? Month,
    DateTimeOffset CreatedAt,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal CarbonCreditsRequired);

public sealed record InventoryActivityEntryResponse(
    Guid Id,
    string CategoryCode,
    string CategoryName,
    EmissionScope Scope,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? EvidenceRef,
    string MetadataJson,
    DateTimeOffset CreatedAt);

public sealed record InventoryCalculationRunResponse(
    Guid Id,
    string FactorSetCode,
    string CalculationVersion,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<InventoryScopeResultResponse> Results);

public sealed record InventoryScopeResultResponse(
    EmissionScope Scope,
    decimal TotalKgCo2e,
    decimal TotalTCo2e);

public sealed record InventoryDetailsResponse(
    Guid Id,
    Guid CompanyId,
    string CompanyLegalName,
    PeriodType PeriodType,
    int Year,
    int? Month,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<InventoryActivityEntryResponse> Entries,
    IReadOnlyCollection<InventoryCalculationRunResponse> CalculationRuns);

public interface IInventoryService
{
    Task<PagedResponse<InventorySummaryResponse>> ListAsync(
        Guid? companyId,
        int? year,
        PeriodType? periodType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<InventoryDetailsResponse?> GetAsync(Guid inventoryId, CancellationToken cancellationToken = default);
}

public interface IInventoryRepository
{
    Task<int> CountAsync(Guid tenantId, Guid? companyId, int? year, PeriodType? periodType, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<InventorySummaryItem>> ListAsync(
        Guid tenantId,
        Guid? companyId,
        int? year,
        PeriodType? periodType,
        int skip,
        int take,
        CancellationToken cancellationToken);
    Task<InventoryDetailsItem?> GetDetailsAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken);
}

public sealed record InventorySummaryItem(
    Guid Id,
    Guid CompanyId,
    string CompanyLegalName,
    PeriodType PeriodType,
    int Year,
    int? Month,
    DateTimeOffset CreatedAt,
    decimal TotalKgCo2e);

public sealed record InventoryDetailsItem(
    Guid Id,
    Guid CompanyId,
    string CompanyLegalName,
    PeriodType PeriodType,
    int Year,
    int? Month,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<InventoryActivityEntryResponse> Entries,
    IReadOnlyCollection<InventoryCalculationRunResponse> CalculationRuns);
