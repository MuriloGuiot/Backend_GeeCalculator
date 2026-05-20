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

public sealed record CreateInventoryRequest(
    Guid CompanyId,
    PeriodType PeriodType,
    int Year,
    int? Month);

public sealed record InventoryActivityEntryResponse(
    Guid Id,
    string CategoryCode,
    string CategoryName,
    EmissionScope Scope,
    string ActivityUnitCode,
    decimal ActivityValue,
    string? SourceName,
    string CalculationMethod,
    string? EvidenceRef,
    string MetadataJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record InventoryCalculationRunResponse(
    Guid Id,
    string FactorSetCode,
    string CalculationVersion,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<InventoryScopeResultResponse> Results);

public sealed record InventoryScopeResultResponse(
    EmissionScope Scope,
    decimal TotalKgCo2e,
    decimal TotalTCo2e,
    decimal BiogenicKgCo2,
    decimal BiogenicTCo2,
    decimal BiogenicRemovalKgCo2,
    decimal BiogenicRemovalTCo2);

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
    Task<InventorySummaryResponse> CreateAsync(CreateInventoryRequest request, CancellationToken cancellationToken = default);

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
    Task<Domain.Entities.Company?> GetCompanyAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken);
    Task<Domain.Entities.EmissionInventory?> GetAsync(Guid tenantId, Guid companyId, PeriodType periodType, int year, int? month, CancellationToken cancellationToken);
    Task AddAsync(Domain.Entities.EmissionInventory inventory, CancellationToken cancellationToken);
    void AddAuditLog(Domain.Entities.AuditLog auditLog);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<InventorySummaryItem?> GetSummaryAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken);
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
