using GEE_Calculator.Application.Calculations;
using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Calculations;
using GEE_Calculator.Domain.Entities;
using GEE_Calculator.Domain.Enums;
using GEE_Calculator.Domain.Tenancy;
using GEE_Calculator.Infrastructure.Auth;

namespace GEE_Calculator.Tests;

public sealed class EmissionCalculationServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid InventoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CompanyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid FactorSetId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid ElectricityCategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01");
    private static readonly Guid LandUseCategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02");
    private static readonly Guid MwhUnitId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01");
    private static readonly Guid Tco2eUnitId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02");

    private static readonly EmissionCalculationService Service = new(
        calculationRepository: null!,
        currentTenantAccessor: null!,
        currentUserContext: null!);

    [Fact]
    public void Preview_ShouldCalculateEmissionTotals()
    {
        var request = new CalculateEmissionPreviewRequest(
            Scope: EmissionScope.Scope2,
            Category: "energia_eletrica_sin",
            ActivityValue: 1500m,
            ActivityUnit: "kWh",
            EmissionFactorKgCo2e: 0.0385m,
            Gwp: 1m);

        var response = Service.Preview(request);

        Assert.Equal(57.75m, response.TotalKgCo2e);
        Assert.Equal(0.05775m, response.TotalTCo2e);
        Assert.Equal(1m, response.CarbonCreditsRequired);
    }

    [Fact]
    public void Preview_ShouldRejectNegativeActivityValue()
    {
        var request = new CalculateEmissionPreviewRequest(
            Scope: EmissionScope.Scope1,
            Category: "diesel_rodoviario",
            ActivityValue: -1m,
            ActivityUnit: "L",
            EmissionFactorKgCo2e: 2.68m,
            Gwp: 1m);

        Assert.Throws<ArgumentOutOfRangeException>(() => Service.Preview(request));
    }

    [Fact]
    public void ComputeSha256_ShouldBeDeterministic()
    {
        var firstHash = ApiKeyValidator.ComputeSha256("gee_dev_local_2026");
        var secondHash = ApiKeyValidator.ComputeSha256("gee_dev_local_2026");

        Assert.Equal(firstHash, secondHash);
        Assert.NotEmpty(firstHash);
    }

    [Fact]
    public async Task CalculateInventoryAsync_ShouldStoreDetailedResultsAndBiogenicTotals()
    {
        var repository = new InMemoryCalculationRepository();
        var service = new EmissionCalculationService(
            repository,
            new FixedTenantAccessor(),
            new FixedCurrentUserContext());

        var response = await service.CalculateInventoryAsync(
            InventoryId,
            new CalculateInventoryRequest("dev_seed_2026"));

        Assert.Equal(67_200_038.5m, response.TotalKgCo2e);
        Assert.Equal(67_200.0385m, response.TotalTCo2e);
        Assert.Equal(2_059_245m, response.TotalBiogenicRemovalKgCo2);
        Assert.Equal(2, repository.Results.Count);
        Assert.All(repository.Results, result => Assert.NotNull(result.ActivityEntryId));
        Assert.Contains(response.ScopeSummaries, item => item.Scope == EmissionScope.Scope1 && item.TotalTCo2e == 67_200m);
        Assert.Contains(response.ScopeSummaries, item => item.Scope == EmissionScope.Scope2 && item.TotalTCo2e == 0.0385m);
        Assert.Contains(response.CategorySummaries, item => item.CategoryCode == "mudanca_uso_solo_ch4" && item.BiogenicRemovalTCo2 == 2059.245m);
    }

    private sealed class FixedTenantAccessor : ICurrentTenantAccessor
    {
        public CurrentTenantSnapshot GetCurrentTenant()
        {
            return new CurrentTenantSnapshot(TenantId.ToString(), null, null, true);
        }
    }

    private sealed class FixedCurrentUserContext : ICurrentUserContext
    {
        public CurrentUserSnapshot GetCurrentUser()
        {
            return new CurrentUserSnapshot("test-user", null, "Test User", TenantId.ToString(), null, [], true);
        }
    }

    private sealed class InMemoryCalculationRepository : IEmissionCalculationRepository
    {
        private readonly EmissionFactorSet factorSet = new()
        {
            Id = FactorSetId,
            SourceId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Code = "dev_seed_2026",
            Name = "Development seed factors 2026",
            VersionLabel = "v2026",
            VersionYear = 2026
        };

        private readonly EmissionFactor electricityFactor = new()
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999901"),
            FactorSetId = FactorSetId,
            CategoryId = ElectricityCategoryId,
            ActivityUnitId = MwhUnitId,
            FactorKgCo2ePerUnit = 38.5m
        };

        public List<CalculationResult> Results { get; } = [];

        public Task<Tenant?> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Tenant?>(new Tenant
            {
                Id = tenantId,
                ExternalTenantId = tenantId.ToString(),
                Name = "Tenant test"
            });
        }

        public Task AddTenantAsync(Tenant tenant, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<CalculationInventoryItem?> GetInventoryForCalculationAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken)
        {
            return Task.FromResult<CalculationInventoryItem?>(new CalculationInventoryItem(
                InventoryId,
                tenantId,
                CompanyId,
                "Empresa Teste",
                PeriodType.Annual,
                2023,
                null));
        }

        public Task<Company?> GetCompanyByExternalIdAsync(Guid tenantId, string externalCompanyId, CancellationToken cancellationToken) => Task.FromResult<Company?>(null);
        public Task<Company?> GetCompanyByTaxIdAsync(Guid tenantId, string taxId, CancellationToken cancellationToken) => Task.FromResult<Company?>(null);
        public Task AddCompanyAsync(Company company, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<EmissionInventory?> GetInventoryAsync(Guid tenantId, Guid companyId, PeriodType periodType, int year, int? month, CancellationToken cancellationToken) => Task.FromResult<EmissionInventory?>(null);
        public Task AddInventoryAsync(EmissionInventory inventory, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<EmissionFactorSet?> GetFactorSetByCodeAsync(string factorSetCode, CancellationToken cancellationToken) => Task.FromResult<EmissionFactorSet?>(factorSet);
        public Task<EmissionFactorSet> GetLatestFactorSetAsync(CancellationToken cancellationToken) => Task.FromResult(factorSet);
        public Task<EmissionCategory?> GetCategoryByCodeAsync(string categoryCode, CancellationToken cancellationToken) => Task.FromResult<EmissionCategory?>(null);
        public Task<ActivityUnit?> GetActivityUnitByCodeAsync(string activityUnitCode, CancellationToken cancellationToken) => Task.FromResult<ActivityUnit?>(null);

        public Task<EmissionFactor?> GetEmissionFactorAsync(Guid factorSetId, Guid categoryId, Guid activityUnitId, Guid tenantId, CancellationToken cancellationToken)
        {
            return Task.FromResult<EmissionFactor?>(
                categoryId == ElectricityCategoryId && activityUnitId == MwhUnitId ? electricityFactor : null);
        }

        public Task<IReadOnlyCollection<CalculationActivityEntry>> ListActiveEntriesAsync(Guid tenantId, Guid inventoryId, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<CalculationActivityEntry> entries =
            [
                new CalculationActivityEntry(
                    Guid.Parse("12121212-1212-1212-1212-121212121201"),
                    ElectricityCategoryId,
                    "energia_eletrica_sin",
                    "Energia eletrica - SIN",
                    EmissionScope.Scope2,
                    MwhUnitId,
                    "MWh",
                    1m,
                    "factor",
                    null,
                    "{}"),
                new CalculationActivityEntry(
                    Guid.Parse("12121212-1212-1212-1212-121212121202"),
                    LandUseCategoryId,
                    "mudanca_uso_solo_ch4",
                    "Mudanca no uso do solo - CH4",
                    EmissionScope.Scope1,
                    Tco2eUnitId,
                    "tCO2e",
                    67_200m,
                    "reported_total",
                    null,
                    "{\"biogenicRemovalTCo2\":2059.245}")
            ];

            return Task.FromResult(entries);
        }

        public Task AddActivityEntryAsync(ActivityEntry activityEntry, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddCalculationRunAsync(CalculationRun calculationRun, CancellationToken cancellationToken) => Task.CompletedTask;
        public void AddCalculationResult(CalculationResult calculationResult) => Results.Add(calculationResult);
        public void AddAuditLog(AuditLog auditLog) { }
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
