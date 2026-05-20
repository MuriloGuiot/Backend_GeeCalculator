using System.Text.Json;
using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.ActivityEntries;
using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Tenancy;

namespace GEE_Calculator.Application.ActivityEntries;

public sealed class ActivityEntryService(
    IActivityEntryRepository activityEntryRepository,
    ICurrentTenantAccessor currentTenantAccessor,
    ICurrentUserContext currentUserContext) : IActivityEntryService
{
    private const string DefaultCalculationMethod = "factor";

    public async Task<IReadOnlyCollection<ActivityEntryResponse>> ListAsync(
        Guid inventoryId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var entries = await activityEntryRepository.ListAsync(tenantId, inventoryId, cancellationToken);
        return entries.Select(ToResponse).ToArray();
    }

    public async Task<ActivityEntryResponse> CreateAsync(
        Guid inventoryId,
        CreateActivityEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        await EnsureInventoryExistsAsync(tenantId, inventoryId, cancellationToken);
        ValidateRequest(request.CategoryCode, request.ActivityUnitCode, request.ActivityValue);

        var category = await ResolveCategoryAsync(request.CategoryCode, cancellationToken);
        var activityUnit = await ResolveActivityUnitAsync(request.ActivityUnitCode, cancellationToken);
        var metadata = NormalizeMetadata(request.MetadataJson);
        var calculationMethod = NormalizeCalculationMethod(request.CalculationMethod);

        var entry = new ActivityEntry
        {
            TenantId = tenantId,
            InventoryId = inventoryId,
            CategoryId = category.Id,
            ActivityUnitId = activityUnit.Id,
            ActivityValue = request.ActivityValue,
            SourceName = NormalizeOptionalText(request.SourceName),
            CalculationMethod = calculationMethod,
            EvidenceRef = NormalizeOptionalText(request.EvidenceRef),
            MetadataJson = metadata
        };

        await activityEntryRepository.AddAsync(entry, cancellationToken);
        activityEntryRepository.AddAuditLog(CreateAuditLog(tenantId, "activity_entry.created", entry.Id, new
        {
            inventoryId,
            categoryCode = category.Code,
            activityUnitCode = activityUnit.Code,
            entry.ActivityValue,
            calculationMethod
        }));
        await activityEntryRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(new ActivityEntryListItem(entry, category, activityUnit));
    }

    public async Task<ActivityEntryResponse?> UpdateAsync(
        Guid inventoryId,
        Guid entryId,
        UpdateActivityEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        ValidateRequest(request.CategoryCode, request.ActivityUnitCode, request.ActivityValue);

        var entry = await activityEntryRepository.GetAsync(tenantId, inventoryId, entryId, cancellationToken);
        if (entry is null)
        {
            return null;
        }

        var category = await ResolveCategoryAsync(request.CategoryCode, cancellationToken);
        var activityUnit = await ResolveActivityUnitAsync(request.ActivityUnitCode, cancellationToken);
        var metadata = NormalizeMetadata(request.MetadataJson);
        var calculationMethod = NormalizeCalculationMethod(request.CalculationMethod);

        entry.CategoryId = category.Id;
        entry.ActivityUnitId = activityUnit.Id;
        entry.ActivityValue = request.ActivityValue;
        entry.SourceName = NormalizeOptionalText(request.SourceName);
        entry.CalculationMethod = calculationMethod;
        entry.EvidenceRef = NormalizeOptionalText(request.EvidenceRef);
        entry.MetadataJson = metadata;
        entry.UpdatedAt = DateTimeOffset.UtcNow;

        activityEntryRepository.AddAuditLog(CreateAuditLog(tenantId, "activity_entry.updated", entry.Id, new
        {
            inventoryId,
            categoryCode = category.Code,
            activityUnitCode = activityUnit.Code,
            entry.ActivityValue,
            calculationMethod
        }));
        await activityEntryRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(new ActivityEntryListItem(entry, category, activityUnit));
    }

    public async Task<bool> DeleteAsync(
        Guid inventoryId,
        Guid entryId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextResolver.ResolveRequiredTenantId(currentTenantAccessor);
        var entry = await activityEntryRepository.GetAsync(tenantId, inventoryId, entryId, cancellationToken);

        if (entry is null)
        {
            return false;
        }

        entry.DeletedAt = DateTimeOffset.UtcNow;
        entry.UpdatedAt = entry.DeletedAt;
        activityEntryRepository.AddAuditLog(CreateAuditLog(tenantId, "activity_entry.deleted", entry.Id, new
        {
            inventoryId
        }));
        await activityEntryRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureInventoryExistsAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken)
    {
        if (!await activityEntryRepository.InventoryExistsAsync(tenantId, inventoryId, cancellationToken))
        {
            throw new ActivityEntryException($"Inventory '{inventoryId}' was not found for the current tenant.");
        }
    }

    private async Task<EmissionCategory> ResolveCategoryAsync(string categoryCode, CancellationToken cancellationToken)
    {
        return await activityEntryRepository.GetCategoryByCodeAsync(categoryCode.Trim(), cancellationToken)
            ?? throw new ActivityEntryException($"Emission category '{categoryCode}' was not found.");
    }

    private async Task<ActivityUnit> ResolveActivityUnitAsync(string activityUnitCode, CancellationToken cancellationToken)
    {
        return await activityEntryRepository.GetActivityUnitByCodeAsync(activityUnitCode.Trim(), cancellationToken)
            ?? throw new ActivityEntryException($"Activity unit '{activityUnitCode}' was not found.");
    }

    private static ActivityEntryResponse ToResponse(ActivityEntryListItem item)
    {
        return new ActivityEntryResponse(
            item.Entry.Id,
            item.Entry.InventoryId,
            item.Category.Code,
            item.Category.Name,
            item.Category.Scope,
            item.ActivityUnit.Code,
            item.Entry.ActivityValue,
            item.Entry.SourceName,
            item.Entry.CalculationMethod,
            item.Entry.EvidenceRef,
            item.Entry.MetadataJson,
            item.Entry.CreatedAt,
            item.Entry.UpdatedAt);
    }

    private static void ValidateRequest(string categoryCode, string activityUnitCode, decimal activityValue)
    {
        if (string.IsNullOrWhiteSpace(categoryCode))
        {
            throw new ActivityEntryException("CategoryCode is required.");
        }

        if (string.IsNullOrWhiteSpace(activityUnitCode))
        {
            throw new ActivityEntryException("ActivityUnitCode is required.");
        }

        if (activityValue < 0)
        {
            throw new ActivityEntryException("ActivityValue cannot be negative.");
        }
    }

    private static string NormalizeMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            return JsonSerializer.Serialize(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new ActivityEntryException($"MetadataJson must be valid JSON. {exception.Message}");
        }
    }

    private static string NormalizeCalculationMethod(string? calculationMethod)
    {
        return string.IsNullOrWhiteSpace(calculationMethod)
            ? DefaultCalculationMethod
            : calculationMethod.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private AuditLog CreateAuditLog(Guid tenantId, string action, Guid entryId, object details)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            ActorExternalUserId = currentUserContext.GetCurrentUser().Subject,
            Action = action,
            EntityName = nameof(ActivityEntry),
            EntityId = entryId.ToString(),
            DetailsJson = JsonSerializer.Serialize(details)
        };
    }
}
