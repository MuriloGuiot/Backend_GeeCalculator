using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Inventories;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Application.Inventories;

public sealed class InventoryService(
    IInventoryRepository inventoryRepository,
    ICurrentTenantAccessor currentTenantAccessor) : IInventoryService
{
    public async Task<PagedResponse<InventorySummaryResponse>> ListAsync(
        Guid? companyId,
        int? year,
        PeriodType? periodType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => pageSize
        };

        var total = await inventoryRepository.CountAsync(tenantId, companyId, year, periodType, cancellationToken);
        var inventories = await inventoryRepository.ListAsync(
            tenantId,
            companyId,
            year,
            periodType,
            (normalizedPage - 1) * normalizedPageSize,
            normalizedPageSize,
            cancellationToken);

        return new PagedResponse<InventorySummaryResponse>(
            inventories.Select(ToSummaryResponse).ToArray(),
            normalizedPage,
            normalizedPageSize,
            total);
    }

    public async Task<InventoryDetailsResponse?> GetAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var inventory = await inventoryRepository.GetDetailsAsync(tenantId, inventoryId, cancellationToken);

        return inventory is null ? null : ToDetailsResponse(inventory);
    }

    private static InventorySummaryResponse ToSummaryResponse(InventorySummaryItem item)
    {
        var totalT = item.TotalKgCo2e / 1000m;
        return new InventorySummaryResponse(
            item.Id,
            item.CompanyId,
            item.CompanyLegalName,
            item.PeriodType,
            item.Year,
            item.Month,
            item.CreatedAt,
            item.TotalKgCo2e,
            totalT,
            Math.Ceiling(totalT));
    }

    private static InventoryDetailsResponse ToDetailsResponse(InventoryDetailsItem item)
    {
        return new InventoryDetailsResponse(
            item.Id,
            item.CompanyId,
            item.CompanyLegalName,
            item.PeriodType,
            item.Year,
            item.Month,
            item.CreatedAt,
            item.Entries,
            item.CalculationRuns);
    }
}
