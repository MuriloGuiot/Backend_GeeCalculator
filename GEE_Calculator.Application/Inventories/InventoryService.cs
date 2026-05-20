using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Common;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Inventories;
using GEE_Calculator.Domain.Tenancy;
using System.Text.Json;

namespace GEE_Calculator.Application.Inventories;

public sealed class InventoryService(
    IInventoryRepository inventoryRepository,
    ICurrentTenantAccessor currentTenantAccessor,
    ICurrentUserContext currentUserContext) : IInventoryService
{
    public async Task<InventorySummaryResponse> CreateAsync(
        CreateInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        ValidateCreateRequest(request);

        var company = await inventoryRepository.GetCompanyAsync(tenantId, request.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Company '{request.CompanyId}' was not found for the current tenant.");

        var inventory = await inventoryRepository.GetAsync(
            tenantId,
            request.CompanyId,
            request.PeriodType,
            request.Year,
            request.Month,
            cancellationToken);

        if (inventory is null)
        {
            inventory = new EmissionInventory
            {
                TenantId = tenantId,
                CompanyId = request.CompanyId,
                PeriodType = request.PeriodType,
                Year = request.Year,
                Month = request.Month
            };

            await inventoryRepository.AddAsync(inventory, cancellationToken);
            inventoryRepository.AddAuditLog(new AuditLog
            {
                TenantId = tenantId,
                ActorExternalUserId = currentUserContext.GetCurrentUser().Subject,
                Action = "inventory.created",
                EntityName = nameof(EmissionInventory),
                EntityId = inventory.Id.ToString(),
                DetailsJson = JsonSerializer.Serialize(new
                {
                    companyId = request.CompanyId,
                    request.PeriodType,
                    request.Year,
                    request.Month
                })
            });
            await inventoryRepository.SaveChangesAsync(cancellationToken);
        }

        var summary = await inventoryRepository.GetSummaryAsync(tenantId, inventory.Id, cancellationToken)
            ?? new InventorySummaryItem(
                inventory.Id,
                company.Id,
                company.LegalName,
                inventory.PeriodType,
                inventory.Year,
                inventory.Month,
                inventory.CreatedAt,
                TotalKgCo2e: 0m);

        return ToSummaryResponse(summary);
    }

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

    private static void ValidateCreateRequest(CreateInventoryRequest request)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is required.");
        }

        if (request.Year is < 2000 or > 2100)
        {
            throw new InvalidOperationException("Year must be between 2000 and 2100.");
        }

        if (!Enum.IsDefined(request.PeriodType))
        {
            throw new InvalidOperationException("PeriodType must be Monthly (1) or Annual (2).");
        }

        if (request.PeriodType == PeriodType.Monthly && request.Month is not >= 1 and <= 12)
        {
            throw new InvalidOperationException("Month must be between 1 and 12 for monthly inventories.");
        }

        if (request.PeriodType == PeriodType.Annual && request.Month is not null)
        {
            throw new InvalidOperationException("Month must be null for annual inventories.");
        }
    }
}
