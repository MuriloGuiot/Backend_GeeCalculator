using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Tenancy;
using System.Globalization;
using System.Text.Json;

namespace GEE_Calculator.Application.Calculations;

public sealed class EmissionCalculationService(
    IEmissionCalculationRepository calculationRepository,
    ICurrentTenantAccessor currentTenantAccessor,
    ICurrentUserContext currentUserContext) : IEmissionCalculationService
{
    private const string CalculationVersion = "gee-v1";

    public CalculateEmissionPreviewResponse Preview(CalculateEmissionPreviewRequest request)
    {
        if (request.ActivityValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.ActivityValue), "Activity value cannot be negative.");
        }

        if (request.EmissionFactorKgCo2e < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.EmissionFactorKgCo2e), "Emission factor cannot be negative.");
        }

        if (request.Gwp <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Gwp), "GWP must be greater than zero.");
        }

        var totalKgCo2e = request.ActivityValue * request.EmissionFactorKgCo2e * request.Gwp;
        var totalTCo2e = totalKgCo2e / 1000m;
        var carbonCreditsRequired = Math.Ceiling(totalTCo2e);

        return new CalculateEmissionPreviewResponse(
            request.Scope,
            request.Category,
            request.ActivityValue,
            request.ActivityUnit,
            request.EmissionFactorKgCo2e,
            request.Gwp,
            totalKgCo2e,
            totalTCo2e,
            carbonCreditsRequired);
    }

    public async Task<RunEmissionCalculationResponse> RunAsync(
        RunEmissionCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRunRequest(request);

        var tenantId = ResolveTenantId();
        var tenant = await EnsureTenantAsync(tenantId, cancellationToken);
        var company = await EnsureCompanyAsync(tenant.Id, request, cancellationToken);
        var inventory = await CreateInventoryAsync(tenant.Id, company.Id, request, cancellationToken);
        var factorSet = await ResolveFactorSetAsync(request.FactorSetCode, cancellationToken);

        foreach (var entryRequest in request.Entries)
        {
            var category = await calculationRepository.GetCategoryByCodeAsync(entryRequest.CategoryCode, cancellationToken)
                ?? throw new EmissionCalculationException($"Emission category '{entryRequest.CategoryCode}' was not found.");

            var unit = await calculationRepository.GetActivityUnitByCodeAsync(entryRequest.ActivityUnitCode, cancellationToken)
                ?? throw new EmissionCalculationException($"Activity unit '{entryRequest.ActivityUnitCode}' was not found.");

            var calculationMethod = NormalizeCalculationMethod(entryRequest.CalculationMethod);
            if (!IsReportedTotal(calculationMethod))
            {
                _ = await calculationRepository.GetEmissionFactorAsync(
                        factorSet.Id,
                        category.Id,
                        unit.Id,
                        tenant.Id,
                        cancellationToken)
                    ?? throw new EmissionCalculationException(
                        $"Emission factor not found for category '{entryRequest.CategoryCode}' and unit '{entryRequest.ActivityUnitCode}' in factor set '{factorSet.Code}'.");
            }

            var activityEntry = new ActivityEntry
            {
                TenantId = tenant.Id,
                InventoryId = inventory.Id,
                CategoryId = category.Id,
                ActivityUnitId = unit.Id,
                ActivityValue = entryRequest.ActivityValue,
                SourceName = NormalizeOptionalText(entryRequest.SourceName),
                CalculationMethod = calculationMethod,
                EvidenceRef = entryRequest.EvidenceRef,
                MetadataJson = NormalizeMetadata(entryRequest.MetadataJson)
            };

            await calculationRepository.AddActivityEntryAsync(activityEntry, cancellationToken);
            calculationRepository.AddAuditLog(CreateAuditLog(
                tenant.Id,
                "activity_entry.created",
                nameof(ActivityEntry),
                activityEntry.Id,
                new
                {
                    inventoryId = inventory.Id,
                    categoryCode = category.Code,
                    activityUnitCode = unit.Code,
                    activityEntry.ActivityValue,
                    activityEntry.CalculationMethod,
                    createdBy = "calculation.run"
                }));
        }

        await calculationRepository.SaveChangesAsync(cancellationToken);

        var inventoryItem = await calculationRepository.GetInventoryForCalculationAsync(
                tenant.Id,
                inventory.Id,
                cancellationToken)
            ?? throw new EmissionCalculationException($"Inventory '{inventory.Id}' was not found.");

        return await CalculateInventoryCoreAsync(tenant.Id, inventoryItem, factorSet, cancellationToken);
    }

    public async Task<RunEmissionCalculationResponse> CalculateInventoryAsync(
        Guid inventoryId,
        CalculateInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = ResolveTenantId();
        await EnsureTenantAsync(tenantId, cancellationToken);

        var inventory = await calculationRepository.GetInventoryForCalculationAsync(
                tenantId,
                inventoryId,
                cancellationToken)
            ?? throw new EmissionCalculationException($"Inventory '{inventoryId}' was not found for the current tenant.");

        var factorSet = await ResolveFactorSetAsync(request.FactorSetCode, cancellationToken);
        return await CalculateInventoryCoreAsync(tenantId, inventory, factorSet, cancellationToken);
    }

    private async Task<RunEmissionCalculationResponse> CalculateInventoryCoreAsync(
        Guid tenantId,
        CalculationInventoryItem inventory,
        EmissionFactorSet factorSet,
        CancellationToken cancellationToken)
    {
        var entries = await calculationRepository.ListActiveEntriesAsync(tenantId, inventory.Id, cancellationToken);
        if (entries.Count == 0)
        {
            throw new EmissionCalculationException("The inventory must contain at least one active activity entry before calculation.");
        }

        var calculationRun = new CalculationRun
        {
            TenantId = tenantId,
            InventoryId = inventory.Id,
            FactorSetId = factorSet.Id,
            CalculationVersion = CalculationVersion
        };

        await calculationRepository.AddCalculationRunAsync(calculationRun, cancellationToken);

        var calculatedEntries = new List<EntryCalculation>();
        foreach (var entry in entries)
        {
            var factor = await ResolveEmissionFactorAsync(entry, factorSet, tenantId, cancellationToken);
            var calculatedEntry = CalculateEntry(entry, factor);
            calculatedEntries.Add(calculatedEntry);

            calculationRepository.AddCalculationResult(new CalculationResult
            {
                TenantId = tenantId,
                CalculationRunId = calculationRun.Id,
                ActivityEntryId = entry.Id,
                Scope = entry.Scope,
                CategoryId = entry.CategoryId,
                GasId = factor?.GasId,
                EmissionFactorId = factor?.Id,
                ActivityUnitId = entry.ActivityUnitId,
                ActivityValue = entry.ActivityValue,
                FactorKgCo2ePerUnit = factor?.FactorKgCo2ePerUnit,
                TotalKgCo2e = calculatedEntry.TotalKgCo2e,
                BiogenicKgCo2 = calculatedEntry.BiogenicKgCo2,
                BiogenicRemovalKgCo2 = calculatedEntry.BiogenicRemovalKgCo2
            });
        }

        calculationRepository.AddAuditLog(CreateAuditLog(
            tenantId,
            "calculation.run",
            nameof(CalculationRun),
            calculationRun.Id,
            new
            {
                inventoryId = inventory.Id,
                inventory.CompanyId,
                factorSetCode = factorSet.Code,
                entries = calculatedEntries.Count,
                totalKgCo2e = calculatedEntries.Sum(item => item.TotalKgCo2e)
            }));

        await calculationRepository.SaveChangesAsync(cancellationToken);

        var scopeSummaries = BuildScopeSummaries(calculatedEntries);
        var categorySummaries = BuildCategorySummaries(calculatedEntries);
        var totalKg = calculatedEntries.Sum(item => item.TotalKgCo2e);
        var totalT = totalKg / 1000m;
        var totalBiogenicKg = calculatedEntries.Sum(item => item.BiogenicKgCo2);
        var totalBiogenicRemovalKg = calculatedEntries.Sum(item => item.BiogenicRemovalKgCo2);

        return new RunEmissionCalculationResponse(
            TenantId: tenantId,
            CompanyId: inventory.CompanyId,
            InventoryId: inventory.Id,
            CalculationRunId: calculationRun.Id,
            FactorSetCode: factorSet.Code,
            TotalKgCo2e: totalKg,
            TotalTCo2e: totalT,
            TotalBiogenicKgCo2: totalBiogenicKg,
            TotalBiogenicTCo2: totalBiogenicKg / 1000m,
            TotalBiogenicRemovalKgCo2: totalBiogenicRemovalKg,
            TotalBiogenicRemovalTCo2: totalBiogenicRemovalKg / 1000m,
            CarbonCreditsRequired: Math.Ceiling(totalT),
            ScopeSummaries: scopeSummaries,
            CategorySummaries: categorySummaries);
    }

    private Guid ResolveTenantId()
    {
        var tenantIdHeader = currentTenantAccessor.GetCurrentTenant().TenantId;

        if (!Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            throw new EmissionCalculationException("The X-Tenant-Id header must be a valid GUID.");
        }

        return tenantId;
    }

    private async Task<Tenant> EnsureTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await calculationRepository.GetTenantAsync(tenantId, cancellationToken);

        if (tenant is not null)
        {
            return tenant;
        }

        tenant = new Tenant
        {
            Id = tenantId,
            ExternalTenantId = tenantId.ToString(),
            Name = $"Tenant {tenantId.ToString()[..8]}"
        };

        await calculationRepository.AddTenantAsync(tenant, cancellationToken);
        return tenant;
    }

    private async Task<Company> EnsureCompanyAsync(
        Guid tenantId,
        RunEmissionCalculationRequest request,
        CancellationToken cancellationToken)
    {
        Company? company = null;

        if (!string.IsNullOrWhiteSpace(request.ExternalCompanyId))
        {
            company = await calculationRepository.GetCompanyByExternalIdAsync(
                tenantId,
                request.ExternalCompanyId,
                cancellationToken);
        }

        if (company is null && !string.IsNullOrWhiteSpace(request.CompanyTaxId))
        {
            company = await calculationRepository.GetCompanyByTaxIdAsync(
                tenantId,
                request.CompanyTaxId,
                cancellationToken);
        }

        if (company is not null)
        {
            return company;
        }

        company = new Company
        {
            TenantId = tenantId,
            LegalName = request.CompanyLegalName,
            TradeName = request.CompanyTradeName,
            TaxId = request.CompanyTaxId,
            ExternalCompanyId = request.ExternalCompanyId
        };

        await calculationRepository.AddCompanyAsync(company, cancellationToken);
        calculationRepository.AddAuditLog(CreateAuditLog(
            tenantId,
            "company.created",
            nameof(Company),
            company.Id,
            new
            {
                company.LegalName,
                company.TaxId,
                company.ExternalCompanyId,
                createdBy = "calculation.run"
            }));
        return company;
    }

    private async Task<EmissionInventory> CreateInventoryAsync(
        Guid tenantId,
        Guid companyId,
        RunEmissionCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var existingInventory = await calculationRepository.GetInventoryAsync(
            tenantId,
            companyId,
            request.PeriodType,
            request.Year,
            request.Month,
            cancellationToken);

        if (existingInventory is not null)
        {
            return existingInventory;
        }

        var inventory = new EmissionInventory
        {
            TenantId = tenantId,
            CompanyId = companyId,
            PeriodType = request.PeriodType,
            Year = request.Year,
            Month = request.Month
        };

        await calculationRepository.AddInventoryAsync(inventory, cancellationToken);
        calculationRepository.AddAuditLog(CreateAuditLog(
            tenantId,
            "inventory.created",
            nameof(EmissionInventory),
            inventory.Id,
            new
            {
                companyId,
                request.PeriodType,
                request.Year,
                request.Month,
                createdBy = "calculation.run"
            }));
        return inventory;
    }

    private async Task<EmissionFactorSet> ResolveFactorSetAsync(string? factorSetCode, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(factorSetCode))
        {
            return await calculationRepository.GetFactorSetByCodeAsync(factorSetCode, cancellationToken)
                ?? throw new EmissionCalculationException($"Factor set '{factorSetCode}' was not found.");
        }

        return await calculationRepository.GetLatestFactorSetAsync(cancellationToken);
    }

    private async Task<EmissionFactor?> ResolveEmissionFactorAsync(
        CalculationActivityEntry entry,
        EmissionFactorSet factorSet,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (IsReportedTotal(entry.CalculationMethod))
        {
            return null;
        }

        return await calculationRepository.GetEmissionFactorAsync(
                factorSet.Id,
                entry.CategoryId,
                entry.ActivityUnitId,
                tenantId,
                cancellationToken)
            ?? throw new EmissionCalculationException(
                $"Emission factor not found for category '{entry.CategoryCode}' and unit '{entry.ActivityUnitCode}' in factor set '{factorSet.Code}'.");
    }

    private static EntryCalculation CalculateEntry(CalculationActivityEntry entry, EmissionFactor? factor)
    {
        using var metadata = ParseMetadata(entry);
        var root = metadata.RootElement;
        var totalKgCo2e = IsReportedTotal(entry.CalculationMethod)
            ? ResolveReportedTotalKgCo2e(entry, root)
            : entry.ActivityValue * (factor?.FactorKgCo2ePerUnit
                ?? throw new EmissionCalculationException("Factor-based calculation requires an emission factor."));

        var biogenicKgCo2 = ReadDecimal(root, "biogenicKgCo2", "biogenic_kg_co2")
            ?? ReadDecimal(root, "biogenicTCo2", "biogenic_t_co2") * 1000m
            ?? 0m;
        var biogenicRemovalKgCo2 = ReadDecimal(root, "biogenicRemovalKgCo2", "biogenic_removal_kg_co2")
            ?? ReadDecimal(root, "biogenicRemovalTCo2", "biogenic_removal_t_co2") * 1000m
            ?? 0m;

        if (totalKgCo2e < 0 || biogenicKgCo2 < 0 || biogenicRemovalKgCo2 < 0)
        {
            throw new EmissionCalculationException("Calculated emission totals cannot be negative.");
        }

        return new EntryCalculation(
            entry.Id,
            entry.Scope,
            entry.CategoryCode,
            entry.CategoryName,
            totalKgCo2e,
            biogenicKgCo2,
            biogenicRemovalKgCo2);
    }

    private static decimal ResolveReportedTotalKgCo2e(CalculationActivityEntry entry, JsonElement metadata)
    {
        var reportedKg = ReadDecimal(metadata, "reportedKgCo2e", "reported_kg_co2e");
        if (reportedKg.HasValue)
        {
            return reportedKg.Value;
        }

        var reportedT = ReadDecimal(metadata, "reportedTCo2e", "reported_t_co2e");
        if (reportedT.HasValue)
        {
            return reportedT.Value * 1000m;
        }

        return entry.ActivityUnitCode switch
        {
            "kgCO2e" => entry.ActivityValue,
            "tCO2e" => entry.ActivityValue * 1000m,
            _ => throw new EmissionCalculationException(
                $"Reported-total entries must use unit 'kgCO2e'/'tCO2e' or include reportedKgCo2e/reportedTCo2e metadata. Entry '{entry.Id}' uses '{entry.ActivityUnitCode}'.")
        };
    }

    private static JsonDocument ParseMetadata(CalculationActivityEntry entry)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(entry.MetadataJson) ? "{}" : entry.MetadataJson);
        }
        catch (JsonException exception)
        {
            throw new EmissionCalculationException($"Activity entry '{entry.Id}' has invalid metadata JSON. {exception.Message}");
        }
    }

    private static decimal? ReadDecimal(JsonElement root, params string[] names)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in root.EnumerateObject())
        {
            if (!names.Any(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.Number &&
                property.Value.TryGetDecimal(out var number))
            {
                return number;
            }

            if (property.Value.ValueKind == JsonValueKind.String &&
                decimal.TryParse(
                    property.Value.GetString(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private static IReadOnlyCollection<ScopeEmissionSummary> BuildScopeSummaries(IReadOnlyCollection<EntryCalculation> calculatedEntries)
    {
        return calculatedEntries
            .GroupBy(item => item.Scope)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var totalKg = group.Sum(item => item.TotalKgCo2e);
                var biogenicKg = group.Sum(item => item.BiogenicKgCo2);
                var biogenicRemovalKg = group.Sum(item => item.BiogenicRemovalKgCo2);

                return new ScopeEmissionSummary(
                    group.Key,
                    totalKg,
                    totalKg / 1000m,
                    biogenicKg,
                    biogenicKg / 1000m,
                    biogenicRemovalKg,
                    biogenicRemovalKg / 1000m);
            })
            .ToArray();
    }

    private static IReadOnlyCollection<CategoryEmissionSummary> BuildCategorySummaries(IReadOnlyCollection<EntryCalculation> calculatedEntries)
    {
        return calculatedEntries
            .GroupBy(item => new { item.Scope, item.CategoryCode, item.CategoryName })
            .OrderBy(group => group.Key.Scope)
            .ThenBy(group => group.Key.CategoryCode)
            .Select(group =>
            {
                var totalKg = group.Sum(item => item.TotalKgCo2e);
                var biogenicKg = group.Sum(item => item.BiogenicKgCo2);
                var biogenicRemovalKg = group.Sum(item => item.BiogenicRemovalKgCo2);

                return new CategoryEmissionSummary(
                    group.Key.Scope,
                    group.Key.CategoryCode,
                    group.Key.CategoryName,
                    totalKg,
                    totalKg / 1000m,
                    biogenicKg,
                    biogenicKg / 1000m,
                    biogenicRemovalKg,
                    biogenicRemovalKg / 1000m);
            })
            .ToArray();
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
            throw new EmissionCalculationException($"MetadataJson must be valid JSON. {exception.Message}");
        }
    }

    private static string NormalizeCalculationMethod(string? calculationMethod)
    {
        return string.IsNullOrWhiteSpace(calculationMethod)
            ? "factor"
            : calculationMethod.Trim().ToLowerInvariant();
    }

    private static bool IsReportedTotal(string calculationMethod)
    {
        return string.Equals(calculationMethod, "reported_total", StringComparison.OrdinalIgnoreCase)
            || string.Equals(calculationMethod, "reported", StringComparison.OrdinalIgnoreCase)
            || string.Equals(calculationMethod, "external_tool", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private AuditLog CreateAuditLog(Guid tenantId, string action, string entityName, Guid entityId, object details)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            ActorExternalUserId = currentUserContext.GetCurrentUser().Subject,
            Action = action,
            EntityName = entityName,
            EntityId = entityId.ToString(),
            DetailsJson = JsonSerializer.Serialize(details)
        };
    }

    private static void ValidateRunRequest(RunEmissionCalculationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyLegalName))
        {
            throw new EmissionCalculationException("CompanyLegalName is required.");
        }

        if (request.Entries.Count == 0)
        {
            throw new EmissionCalculationException("At least one activity entry is required.");
        }

        if (request.Year < 2000 || request.Year > 2100)
        {
            throw new EmissionCalculationException("Year must be between 2000 and 2100.");
        }

        if (!Enum.IsDefined(request.PeriodType))
        {
            throw new EmissionCalculationException("PeriodType must be Monthly (1) or Annual (2).");
        }

        if (request.PeriodType == PeriodType.Monthly && request.Month is not >= 1 and <= 12)
        {
            throw new EmissionCalculationException("Month must be between 1 and 12 for monthly inventories.");
        }

        if (request.PeriodType == PeriodType.Annual && request.Month is not null)
        {
            throw new EmissionCalculationException("Month must be null for annual inventories.");
        }

        foreach (var entry in request.Entries)
        {
            if (entry.ActivityValue < 0)
            {
                throw new EmissionCalculationException("Activity values cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(entry.CategoryCode) || string.IsNullOrWhiteSpace(entry.ActivityUnitCode))
            {
                throw new EmissionCalculationException("CategoryCode and ActivityUnitCode are required for every entry.");
            }
        }
    }

    private sealed record EntryCalculation(
        Guid ActivityEntryId,
        EmissionScope Scope,
        string CategoryCode,
        string CategoryName,
        decimal TotalKgCo2e,
        decimal BiogenicKgCo2,
        decimal BiogenicRemovalKgCo2);
}
