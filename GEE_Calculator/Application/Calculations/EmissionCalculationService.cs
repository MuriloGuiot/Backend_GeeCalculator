using GEE_Calculator.Application.Tenancy;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEE_Calculator.Application.Calculations;

public sealed class EmissionCalculationService(
    GeeCalculatorDbContext dbContext,
    ICurrentTenantAccessor currentTenantAccessor) : IEmissionCalculationService
{
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

        var groupedResults = new Dictionary<EmissionScope, decimal>();

        foreach (var entryRequest in request.Entries)
        {
            var category = await dbContext.EmissionCategories
                .SingleOrDefaultAsync(item => item.Code == entryRequest.CategoryCode, cancellationToken)
                ?? throw new EmissionCalculationException($"Emission category '{entryRequest.CategoryCode}' was not found.");

            var unit = await dbContext.ActivityUnits
                .SingleOrDefaultAsync(item => item.Code == entryRequest.ActivityUnitCode, cancellationToken)
                ?? throw new EmissionCalculationException($"Activity unit '{entryRequest.ActivityUnitCode}' was not found.");

            var factor = await dbContext.EmissionFactors
                .Where(item => item.FactorSetId == factorSet.Id
                    && item.CategoryId == category.Id
                    && item.ActivityUnitId == unit.Id
                    && item.TenantId == null)
                .OrderBy(item => item.CreatedAt)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new EmissionCalculationException(
                    $"Emission factor not found for category '{entryRequest.CategoryCode}' and unit '{entryRequest.ActivityUnitCode}'.");

            var activityEntry = new ActivityEntry
            {
                TenantId = tenant.Id,
                InventoryId = inventory.Id,
                CategoryId = category.Id,
                ActivityUnitId = unit.Id,
                ActivityValue = entryRequest.ActivityValue,
                EvidenceRef = entryRequest.EvidenceRef,
                MetadataJson = string.IsNullOrWhiteSpace(entryRequest.MetadataJson) ? "{}" : entryRequest.MetadataJson
            };

            await dbContext.ActivityEntries.AddAsync(activityEntry, cancellationToken);

            var totalKgCo2e = entryRequest.ActivityValue * factor.FactorKgCo2ePerUnit;
            groupedResults[category.Scope] = groupedResults.GetValueOrDefault(category.Scope) + totalKgCo2e;
        }

        var calculationRun = new CalculationRun
        {
            TenantId = tenant.Id,
            InventoryId = inventory.Id,
            FactorSetId = factorSet.Id,
            CalculationVersion = "gee-v0"
        };

        await dbContext.CalculationRuns.AddAsync(calculationRun, cancellationToken);

        var scopeSummaries = groupedResults
            .OrderBy(item => item.Key)
            .Select(item =>
            {
                var result = new CalculationResult
                {
                    TenantId = tenant.Id,
                    CalculationRunId = calculationRun.Id,
                    Scope = item.Key,
                    TotalKgCo2e = item.Value
                };

                dbContext.CalculationResults.Add(result);

                return new ScopeEmissionSummary(
                    Scope: item.Key,
                    TotalKgCo2e: item.Value,
                    TotalTCo2e: item.Value / 1000m);
            })
            .ToArray();

        dbContext.AuditLogs.Add(new AuditLog
        {
            TenantId = tenant.Id,
            Action = "calculation.run",
            EntityName = nameof(CalculationRun),
            EntityId = calculationRun.Id.ToString()
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var totalKg = scopeSummaries.Sum(item => item.TotalKgCo2e);
        var totalT = totalKg / 1000m;

        return new RunEmissionCalculationResponse(
            TenantId: tenant.Id,
            CompanyId: company.Id,
            InventoryId: inventory.Id,
            CalculationRunId: calculationRun.Id,
            FactorSetCode: factorSet.Code,
            TotalKgCo2e: totalKg,
            TotalTCo2e: totalT,
            CarbonCreditsRequired: Math.Ceiling(totalT),
            ScopeSummaries: scopeSummaries);
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
        var tenant = await dbContext.Tenants.SingleOrDefaultAsync(item => item.Id == tenantId, cancellationToken);

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

        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
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
            company = await dbContext.Companies.SingleOrDefaultAsync(
                item => item.TenantId == tenantId && item.ExternalCompanyId == request.ExternalCompanyId,
                cancellationToken);
        }

        if (company is null && !string.IsNullOrWhiteSpace(request.CompanyTaxId))
        {
            company = await dbContext.Companies.SingleOrDefaultAsync(
                item => item.TenantId == tenantId && item.TaxId == request.CompanyTaxId,
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

        await dbContext.Companies.AddAsync(company, cancellationToken);
        return company;
    }

    private async Task<EmissionInventory> CreateInventoryAsync(
        Guid tenantId,
        Guid companyId,
        RunEmissionCalculationRequest request,
        CancellationToken cancellationToken)
    {
        var inventory = new EmissionInventory
        {
            TenantId = tenantId,
            CompanyId = companyId,
            PeriodType = request.PeriodType,
            Year = request.Year,
            Month = request.Month
        };

        await dbContext.EmissionInventories.AddAsync(inventory, cancellationToken);
        return inventory;
    }

    private async Task<EmissionFactorSet> ResolveFactorSetAsync(string? factorSetCode, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(factorSetCode))
        {
            return await dbContext.EmissionFactorSets
                .SingleOrDefaultAsync(item => item.Code == factorSetCode, cancellationToken)
                ?? throw new EmissionCalculationException($"Factor set '{factorSetCode}' was not found.");
        }

        return await dbContext.EmissionFactorSets
            .OrderByDescending(item => item.VersionYear)
            .ThenBy(item => item.Code)
            .FirstAsync(cancellationToken);
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
}
